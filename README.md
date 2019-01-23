# ExternalSQL.Net
类似于mybatis，在xml中保存sql语句，实现sql语句的外置，同时可以使用多种节点实现sql语句的复用和参数化等基本功能<br/><br/>

如何使用？<br/>
一、下载源码或者下载release的dll文件，引入到主项目中。<br/>
二、主项目config中的&lt;configSections> 添加<br/>
    &nbsp;&nbsp;&nbsp;&nbsp;&lt;section name="sqlPaths" type="ExternalSQL.Net.PathHandler" /><br/>
三、在config根节点<configuration>中添加<br/>
    &nbsp;&nbsp;&nbsp;&nbsp;&lt;sqlPaths><br/>
       &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;add key="sqlserver" value="SQLS" /><br/>
    &nbsp;&nbsp;&nbsp;&nbsp;&lt;/sqlPaths><br/>
    &nbsp;&nbsp;&nbsp;&nbsp;<sqlPaths>名称不可更改，key为保存sql语句的xml的路径的主键，value为保存sql语句的xml的路径，可配置多个<br/>
四、在相应路径中新增xml文件，根节点为root<br/>
五、在root下新增<br/>
    &nbsp;&nbsp;&nbsp;&nbsp;&lt;sql id="isexistsuser"><br/>
       &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;select * from EmplyeeInfo e<br/>
       &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;where e.username=#username#<br/>
       &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;and e.userpassword=@userpassword<br/>
       &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;and e.countup=$countup$<br/>
       &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;include target="getuserparas" /><br/>
       &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;and userid='110'<br/>
       &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;parameter>username,userpassword&lt;/parameter><br/>
       &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;parameter>countup&lt;/parameter><br/>
    &nbsp;&nbsp;&nbsp;&nbsp;&lt;/sql><br/>
    &nbsp;&nbsp;&nbsp;&nbsp;&lt;sql id="getuserparas"><br/>
       &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;and customid=@customid<br/>
       &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;parameter>customid&lt;/parameter><br/>
    &nbsp;&nbsp;&nbsp;&nbsp;&lt;/sql><br/>
    &nbsp;&nbsp;&nbsp;&nbsp;一个sql节点表示一个sql语句。<br/>
    &nbsp;&nbsp;&nbsp;&nbsp;##在生成时会替换成''。<br/>
    &nbsp;&nbsp;&nbsp;&nbsp;$$会直接替换成对象的值。<br/>
    &nbsp;&nbsp;&nbsp;&nbsp;@直接使用sqlserver的参数化防止sql注入。<br/>
    &nbsp;&nbsp;&nbsp;&nbsp;parameter中的值与语句中的参数一一对应，支持一个节点中以,分割的形式添加多个参数。<br/>
    &nbsp;&nbsp;&nbsp;&nbsp;解析时parameter会与传入的数据对象一一对应<br/>
六、ExternalSQL.Net中的SqlserverHelper中存在各种入口可以直接调用。<br/>
    &nbsp;&nbsp;&nbsp;&nbsp;需要传入的参数主要为<br/>
    &nbsp;&nbsp;&nbsp;&nbsp;string pathName(第三步中的某个add节点中的key)<br/>
    &nbsp;&nbsp;&nbsp;&nbsp;string docName(调用的xml文件名,不需要后缀)<br/>
    &nbsp;&nbsp;&nbsp;&nbsp;string sqlId(xml文件中sql节点的id)<br/>
    &nbsp;&nbsp;&nbsp;&nbsp;object paras(对应语句中需要接收的参数键值对对象)<br/>
    &nbsp;&nbsp;&nbsp;&nbsp;string conStr(config中connectionStrings节点中的连接字符串的name)<br/>
七、返回数据<br/>
