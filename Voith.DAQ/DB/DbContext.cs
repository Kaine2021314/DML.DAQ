using SqlSugar;
using System;
using System.Linq;
using Voith.DAQ.Model;

namespace Voith.DAQ.DB
{
    class DbContext
    {
        public DbContext()
        {
            Db = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = new Common.JsonConfigHelper("Config.json")["DBString"],
                DbType = DbType.SqlServer,
                InitKeyType = InitKeyType.Attribute,//从特性读取主键和自增列信息
                IsAutoCloseConnection = true

            });

            //调式代码 用来打印SQL 
            Db.Aop.OnLogExecuting = (sql, pars) =>
            {
                Console.WriteLine(sql + "\r\n" +
                    Db.Utilities.SerializeObject(pars.ToDictionary(it => it.ParameterName, it => it.Value)));
                Console.WriteLine();
            };

        }
        //注意：不能写成静态的，不能写成静态的
        //用来处理事务多表查询和复杂的操作
        public SqlSugarClient Db;

        public SimpleClient<ProductionOrder> ProductionOrderDb { get { return new SimpleClient<ProductionOrder>(Db); } }
        public SimpleClient<GoodsOrder> GoodsOrderDb { get { return new SimpleClient<GoodsOrder>(Db); } }
        public SimpleClient<Formula> FormulaDb { get { return new SimpleClient<Formula>(Db); } }

        public SimpleClient<GoodsOrder80> GoodsOrder80Db { get { return new SimpleClient<GoodsOrder80>(Db); } }
    }
}
