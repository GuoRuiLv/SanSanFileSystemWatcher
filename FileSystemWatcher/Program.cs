using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;


namespace SanSan
{
    class Program
    {
        private static FileSystemWatcher m_Watcher = null;
        private static List<String> distributionPath = new List<string>();
        private static string SanSanSrcPath = null;
        static void Main(string[] args)
        {
            Console.Title = Properties.Settings.Default.Title?? "文件同步分发器";
            SanSanSrcPath =  Properties.Settings.Default.SrcPath??"";
            
            if (!Directory.Exists(SanSanSrcPath))
            {
                Console.WriteLine("源目录不存在:"+SanSanSrcPath);
                Console.Read();
                return;
                

            }
            Console.WriteLine("监控目录:" + SanSanSrcPath);
            m_Watcher = new FileSystemWatcher();
            m_Watcher.Path = SanSanSrcPath;
            m_Watcher.IncludeSubdirectories = true;
            m_Watcher.Filter = "";
            
            m_Watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName|NotifyFilters.DirectoryName;
            m_Watcher.Created += OnChanged;
            m_Watcher.Changed +=OnChanged;
            m_Watcher.Deleted +=OnChanged;
            m_Watcher.Renamed += OnRenamed;
            m_Watcher.EnableRaisingEvents = true;
            createDistPath();
            Console.WriteLine("按下\'Q\' 退出监控!");
            while (Console.Read() != 'q')
            {
                if (Console.Read() == 'r' || Console.Read() == 'R')
                {
                    Console.WriteLine("将完整分发源码");                    
                }
            }
        }


        private static void reNameToDistPath(String oldFullPath,String srcFullPath)
        {
            distributionPath.ForEach(
                (f) =>
                {

                    String date = DateTime.Now.ToString();
                    string sPath = getDistPath(f, oldFullPath);
                    string dPath = getDistPath(f, srcFullPath);
                    try
                    {
                        if (Directory.Exists(sPath))
                        {
                            Console.WriteLine(date+" 重命名目录:"+sPath+" TO "+dPath);
                            Directory.Move(sPath,dPath);
                        }
                        else
                        {
                            Console.WriteLine(date + " 重命名文件:" + sPath + " TO " + dPath);
                            File.Move(sPath,dPath);
                        }                        
                      
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(date + " 重命名失败:" + ex.Message);
                    }


                }
        );
        }


        private static void OnRenamed(object sender, RenamedEventArgs e)
        {
           reNameToDistPath(e.OldFullPath,e.FullPath);
        }

       

        private static void createDistPath()
        {
            Console.WriteLine("将分发到以下目录:");
            
            string distPahts = Properties.Settings.Default.DistributionPath;
            string[] a = distPahts.Split(';');
            for (int i = 0; i < a.Length; i++)
            {
                var f = a[i];
                if (Directory.Exists(f))
                {
                    Console.WriteLine(f);
                    distributionPath.Add(f);
                }
                else
                {
                    Console.WriteLine("分发路径："+f+"不存在");
                }
            }
        }

        private static string getDistPath(String distPath,String srcFullPath)
        {
            String relaPath = srcFullPath.Substring(SanSanSrcPath.Length);
            if (relaPath[0]=='\\')
            {
                relaPath = relaPath.Substring(1);
            }
            string p = Path.Combine(distPath, relaPath);
            return p;
        }

        private static void copyToDistPath(String srcFullPath)
        {
            distributionPath.ForEach(
                (f)=>
            {
                
                String date = DateTime.Now.ToString();
                string dPath = getDistPath(f, srcFullPath);
                try
                {
                    //Directory.CreateDirectory(dPath);
                    if (File.Exists(dPath))
                    {
                        File.Delete(dPath);
                    }
                    File.Copy(srcFullPath, dPath);
                    Console.WriteLine(date+" 复制:"+srcFullPath+"到"+dPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(date + " 复制:" + srcFullPath + "到" + dPath+"失败:"+ex.Message);
                }
                

            }
        );
        }

        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
            
            String date = DateTime.Now.ToString();
            Console.WriteLine( date+ " {0} {1}", e.ChangeType, e.FullPath);
            
            if (e.ChangeType==WatcherChangeTypes.Changed)
            {
                if (Directory.Exists(e.FullPath))
                {
                    //是一个目录

                }
                else
                {
                    //是文件
                    copyToDistPath(e.FullPath);    
                }
                

            }else if (e.ChangeType == WatcherChangeTypes.Created)
            {
                if (Directory.Exists(e.FullPath))
                {
                    //是一个目录
                    createDict(e.FullPath);
                }
                else
                {
                    copyToDistPath(e.FullPath);    
                }
                
            }
            else if (e.ChangeType == WatcherChangeTypes.Deleted)
            {
                if (Directory.Exists(e.FullPath))
                {
                    //是一个目录
                    deleteDict(e.FullPath);
                }
                {
                    deleteDistPath(e.FullPath);
                }
            }
            else if (e.ChangeType == WatcherChangeTypes.Renamed)
            {
                ;
            }

        }

        private static void deleteDict(string srcFullPath)
        {
            distributionPath.ForEach(
                (f) =>
                {
                    String date = DateTime.Now.ToString();
                    string dPath = getDistPath(f, srcFullPath);
                    try
                    {
                        
                        Console.WriteLine(date + " 删除:" + dPath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(date + " 删除:" + dPath + "失败:" + ex.Message);
                    }

                });
        }

        private static void createDict(string srcFullPath)
        {
            distributionPath.ForEach(
                (f) =>
                {
                    String date = DateTime.Now.ToString();
                    string dPath = getDistPath(f, srcFullPath);
                    try
                    {
                        Directory.CreateDirectory(dPath);
                        Console.WriteLine(date + " 创建:" + dPath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(date + " 创建:" + dPath + "失败:" + ex.Message);
                    }

                });
        }

        private static void deleteDistPath(string srcFullPath)
        {
            distributionPath.ForEach(
                (f) =>
                {
                    String date = DateTime.Now.ToString();
                    string dPath = getDistPath(f, srcFullPath);
                    try
                    {
                        
                        
                        File.Delete(dPath);
                        Console.WriteLine(date + " 删除:" + dPath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(date + " 删除:" + dPath+"失败:"+ex.Message);
                    }
                    
                });
        }
    }
}
