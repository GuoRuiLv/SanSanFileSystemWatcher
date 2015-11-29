#SanSanFileSystemWatcher
文件系统监控分发器 监控指定的目录文件，将所有文件的改分发到多个目标目录。 初次用于一个JavaScript项目的源码同步。
可配置需要监控的目录和要分发到的目录，支持多个分发目录。

配置参数说明:
DistributionPath:分发目录，多个分发目录之间用分号分隔
SrcPath:要监控的目录
Title:控制台窗口标题
IngorePattern:要忽略的路径模式，符合此模式的文件或文件夹将不做处理,默认是\.git[\s\S]* ，表示忽略.git目录

![文件监控分发器](http://git.oschina.net/uploads/images/2015/1129/184354_a5567dcf_516161.png "文件监控分发器")