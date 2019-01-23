# ExternalSQL.Net
类似于mybatis，在xml中保存sql语句，实现sql语句的外置，同时可以使用多种节点实现sql语句的复用和参数化等基本功能

如何使用？
一、下载源码或者下载release的dll文件，引入到主项目中。
二、主项目config中的<configSections> 添加
    <section name="sqlPaths" type="ExternalSQL.Net.PathHandler" />
三、在config根节点<configuration>中添加
    <sqlPaths>
       <add key="sqlserver" value="SQLS"></add>
    </sqlPaths>
    <sqlPaths>名称不可更改，key为保存sql语句的xml的路径的主键，value为保存sql语句的xml的路径，可配置多个
四、在相应路径中新增xml文件，根节点为root
五、在root下新增
    <sql id="isexistsuser">
       select * from EmplyeeInfo e
       where e.username=#username#
       and e.userpassword=@userpassword
       and e.countup=$countup$
       <include target="getuserparas"></include>
       and userid='110'
       <parameter>username,userpassword</parameter>
       <parameter>countup</parameter>
    </sql>
    <sql id="getuserparas">
       and customid=@customid
       <parameter>customid</parameter>
    </sql>
    一个sql节点表示一个sql语句。
    ##在生成时会替换成''。
    $$会直接替换成对象的值。
    @直接使用sqlserver的参数化防止sql注入。
    parameter中的值与语句中的参数一一对应，支持一个节点中以,分割的形式添加多个参数。
    解析时parameter会与传入的数据对象一一对应
六、ExternalSQL.Net中的SqlserverHelper中存在各种入口可以直接调用。
    需要传入的参数主要为
    string pathName(第三步中的某个add节点中的key)
    string docName(调用的xml文件名,不需要后缀)
    string sqlId(xml文件中sql节点的id)
    object paras(对应语句中需要接收的参数键值对对象)
    string conStr(config中connectionStrings节点中的连接字符串的name)
七、返回数据
