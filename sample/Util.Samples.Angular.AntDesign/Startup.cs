﻿using System;
using System.IO;
using EasyCaching.InMemory;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Util.Caches.EasyCaching;
using Util.Events.Cap;
using Util.Events.Default;
using Util.Locks.Default;
using Util.Logs.Extensions;
using Util.Ui.Extensions;
using Util.Webs.Extensions;

namespace Util.Samples {
    /// <summary>
    /// 启动配置
    /// </summary>
    public class Startup {
        /// <summary>
        /// 初始化启动配置
        /// </summary>
        /// <param name="configuration">配置</param>
        public Startup( IConfiguration configuration ) {
            Configuration = configuration;
        }

        /// <summary>
        /// 配置
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// 配置服务
        /// </summary>
        public IServiceProvider ConfigureServices( IServiceCollection services ) {
            //注册Razor视图解析路径
            services.AddRazorViewLocationExpander();

            //添加Mvc服务
            services.AddMvc( options => {
                //options.Filters.Add( new AutoValidateAntiforgeryTokenAttribute() );
            } ).SetCompatibilityVersion( CompatibilityVersion.Version_2_2 )
               .AddRazorPageConventions();

            //添加NLog日志操作
            services.AddNLog();

            //添加EasyCaching缓存
            services.AddCache( options => options.UseInMemory() );

            //添加业务锁
            services.AddLock();

            //注册XSRF令牌服务
            services.AddXsrfToken();

            //添加EF工作单元
            //====== 支持Sql Server 2012+ ==========
            //services.AddUnitOfWork<ISampleUnitOfWork, Util.Samples.Webs.Data.SqlServer.SampleUnitOfWork>( Configuration.GetConnectionString( "DefaultConnection" ) );
            //======= 支持Sql Server 2005+ ==========
            //services.AddUnitOfWork<ISampleUnitOfWork, Util.Samples.Webs.Data.SqlServer.SampleUnitOfWork>( builder => {
            //    builder.UseSqlServer( Configuration.GetConnectionString( "DefaultConnection" ), option => option.UseRowNumberForPaging() );
            //} );
            //======= 支持PgSql =======
            //services.AddUnitOfWork<ISampleUnitOfWork, Util.Samples.Webs.Data.PgSql.SampleUnitOfWork>( Configuration.GetConnectionString( "PgSqlConnection" ) );
            //======= 支持MySql =======
            //services.AddUnitOfWork<ISampleUnitOfWork, Util.Samples.Webs.Data.MySql.SampleUnitOfWork>( Configuration.GetConnectionString( "MySqlConnection" ) );

            //添加Swagger
            services.AddSwaggerGen( options => {
                options.SwaggerDoc( "v1", new Info { Title = "Util Api Demo", Version = "v1" } );
                options.IncludeXmlComments( Path.Combine( AppContext.BaseDirectory, "Util.xml" ) );
                options.IncludeXmlComments( Path.Combine( AppContext.BaseDirectory, "Util.Webs.xml" ) );
                options.IncludeXmlComments( Path.Combine( AppContext.BaseDirectory, "Util.Samples.xml" ) );
            } );

            //添加事件总线
            services.AddEventBus();

            //添加Cap事件总线
            //services.AddEventBus( options => {
            //    options.UseDashboard();
            //    options.UseSqlServer( Configuration.GetConnectionString( "DefaultConnection" ) );
            //    options.UseRabbitMQ( "192.168.244.138" );
            //} );

            //添加Util基础设施服务
            return services.AddUtil();
        }

        /// <summary>
        /// 配置开发环境请求管道
        /// </summary>
        public void ConfigureDevelopment( IApplicationBuilder app ) {
            app.UseBrowserLink();
            app.UseDeveloperExceptionPage();
            app.UseDatabaseErrorPage();
            app.UseWebpackDevMiddleware( new WebpackDevMiddlewareOptions {
                HotModuleReplacement = true
            } );
            app.UseSwaggerX();
            CommonConfig( app );
        }

        /// <summary>
        /// 配置生产环境请求管道
        /// </summary>
        public void ConfigureProduction( IApplicationBuilder app ) {
            app.UseExceptionHandler( "/Home/Error" );
            CommonConfig( app );
        }

        /// <summary>
        /// 公共配置
        /// </summary>
        private void CommonConfig( IApplicationBuilder app ) {
            app.UseErrorLog();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseXsrfToken();
            ConfigRoute( app );
        }

        /// <summary>
        /// 路由配置,支持区域
        /// </summary>
        private void ConfigRoute( IApplicationBuilder app ) {
            app.UseMvc( routes => {
                routes.MapSpaFallbackRoute( "spa-fallback", new { controller = "Home", action = "Index" } );
            } );
        }
    }
}