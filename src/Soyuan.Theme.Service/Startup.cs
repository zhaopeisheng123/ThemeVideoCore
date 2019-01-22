using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Soyuan.Theme.Business;
using Soyuan.Theme.Contracts;
using Soyuan.Theme.Core.Helper;
using Soyuan.Theme.Domain;
using Soyuan.Theme.Service.core;
using Soyuan.Theme.Service.Services.Application;
using Soyuan.Theme.Service.Services.Organization;
using Soyuan.Theme.Service.Services.UploadHand;
using Soyuan.Theme.Service.Services.User;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soyuan.Theme.Service
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

        }

        public IConfiguration Configuration { get; }
        public IContainer ApplicationContainer { get; private set; }
        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            //配置jwt
            ConfigureJwt(services);
            services.AddCors(options =>
             options.AddPolicy("default",
             p => p.AllowAnyOrigin()
              .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
             )
             );
            services.AddMvc();
            

            var builder = new ContainerBuilder();//实例化 AutoFac  容器           
            builder.Populate(services);
            builder.RegisterType<UploadHandService>().As<IUploadHandService>();
            builder.RegisterType<OrganizationService>().As<IOrganizationService>();
            builder.RegisterType<UserService>().As<IUserService>();
            builder.RegisterType<ApplicationService>().As<IApplicationService>();
            builder.RegisterType<ThemeDBContext>().InstancePerDependency();
            ApplicationContainer = builder.Build();

            return new AutofacServiceProvider(ApplicationContainer);//第三方IOC接管 core内置DI容器
        }

        /// <summary>
        /// 配置jwt
        /// </summary>
        /// <param name="services"></param>
        private void ConfigureJwt(IServiceCollection services)
        {
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                string appId;
                o.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        string jtiString = context.Principal.Claims.FirstOrDefault(i => i.Type == "Jti")?.Value;
                        //if (JWT.Helper.IsInBlackList(jtiString))
                        //{
                        //    context.Fail("令牌已被注销");
                        //}
                        return Task.CompletedTask;
                    },
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Headers["token"];
                        context.Token = token.FirstOrDefault();
                        appId = context.Request.Headers["appId"];
                        AddTokenValidation(o, context, appId);
                        return Task.CompletedTask;
                    }
                };

                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidIssuer = "ThemeVideo",
                    ValidateAudience = true,
                    ValidateLifetime = true, //validate the expiration and not before values in the token
                    ClockSkew = TimeSpan.FromSeconds(5) //5 minute tolerance for the expiration date
                };
            });
        }


        /// <summary>
        /// 检验系统是否存在，如果存在，增加token验证规则
        /// </summary>
        /// <param name="o">jwt验证选项</param>
        /// <param name="context">信息接收上下文</param>
        /// <param name="appIdString">系统主键</param>
        private void AddTokenValidation(JwtBearerOptions o, MessageReceivedContext context, string appIdString)
        {
            if (!Guid.TryParse(appIdString, out Guid appId))
            {
                //决绝访问
                context.Fail("错误的系统标识");
                return;
            }

            var app = new ApplicationService(new ThemeDBContext()).GetApplicationByID(appId);
            if (app == null)

            {
                //决绝访问
                context.Fail("系统不存在或已被禁用");
                return;
            }
            o.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(app.AppSecret));
            o.TokenValidationParameters.ValidAudience = app.AppName;
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime lifetime)
        {
            app.UseAuthentication();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseCors("default");
            app.UseMvc();  

            var confRegisterConsul = ConfigHelper.ReadConfigByName("IsRegisterConsul", "");
            var IsRegisterConsul = false;
            Boolean.TryParse(confRegisterConsul, out IsRegisterConsul);

            //注册服务
            if (IsRegisterConsul)
            {
                ServiceContract serviceEntity = new ServiceContract
                {
                    IP = Configuration["Service:Ip"],
                    Port = Convert.ToInt32(Configuration["Service:Port"]),
                    ServiceName = Configuration["Service:Name"],
                    ConsulIP = Configuration["Consul:IP"],
                    ConsulPort = Convert.ToInt32(Configuration["Consul:Port"])
                };
                ConsulCommon.RegisterConsul(app, lifetime, serviceEntity);
            }

            string KafkaUploadTopic = ConfigHelper.ReadConfigByName("KafkaUploadTopic");
            string KafkaMsgTopic = ConfigHelper.ReadConfigByName("KafkaMsgTopic");
            UploadLogic uploadLogic = new UploadLogic();


            //总署订阅消息
            KafKaLogic.GetInstance().Pull((KafKaContract, topic) =>
                 {
                     if (topic == KafkaUploadTopic)  //总署订阅消息
                     {
                         Task.Run(() =>
                         {
                             uploadLogic.PullUploadMsg(KafKaContract);
                         });
                     }
                     else if (topic == KafkaMsgTopic) //隶属关订阅消息
                     {
                         Task.Run(() =>
                         {
                             uploadLogic.PullMsg(KafKaContract);
                         });
                     }
                 });

            var model = new UploadContract();
            model.InsertTime = DateTime.Now;

            var str = SerializeHelper.serializeToString(model);

            LogHelper.logInfo(str);
            //定时任务
            QuartzLogic quartz = new QuartzLogic();
            var cron = ConfigHelper.ReadConfigByName("QuartzTime");
            quartz.ExecuteByCron<MyJobLogic>(cron);
        }
    }
}
