using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Net.Configuration;
using System.Text.RegularExpressions;


namespace SanSan
{
    class Program
    {
        private static FileSystemWatcher m_Watcher = null;
        private static List<String> distributionPath = new List<string>();
        private static string SanSanSrcPath = null;
        private static string IngorePattern=null;
        private static List<Regex> IngoreRegex=new List<Regex>();

        static void Main(string[] args)
        {
            
            Console.Title = Properties.Settings.Default.Title?? "文件同步分发器";
            SanSanSrcPath =  Properties.Settings.Default.SrcPath??"";
            if (SanSanSrcPath.EndsWith("\\"))
            {
                SanSanSrcPath = SanSanSrcPath.Substring(0, SanSanSrcPath.Length - 1);
            }
            IngorePattern = Properties.Settings.Default.IngorePattern??"";

            var a = IngorePattern.Split('\r');
            for (int i= 0; i <a.Length ; i++)
            {
                if (String.IsNullOrEmpty(a[i]))
                {
                    continue;                    
                }
                try
                {
                    var reg = new Regex(a[i]);
                    IngoreRegex.Add(reg);
                }
                catch (Exception e)
                {

                    Console.Write("初始化忽略规则:" + a[i] + "失败" + e.Message);
                }
                
            }

            
            
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

            m_Watcher.NotifyFilter =
                NotifyFilters.LastWrite
                | NotifyFilters.FileName
                | NotifyFilters.DirectoryName;
                //|NotifyFilters.Attributes
                //|NotifyFilters.CreationTime
                //|NotifyFilters.LastAccess
                //|NotifyFilters.Security
                //|NotifyFilters.Size;
            
            m_Watcher.Created += OnChanged;
            m_Watcher.Changed +=OnChanged;
            m_Watcher.Deleted +=OnChanged;
            m_Watcher.Renamed += OnRenamed;
            m_Watcher.Error += OnError;
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

        private static void OnError(object sender, ErrorEventArgs e)
        {
            String date = DateTime.Now.ToString();
            Console.WriteLine(date + "Error: {0} ", e.GetException().Message);
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

        private static bool isIngorePath(String fullPath)
        {
            var r=IngoreRegex.Exists((reg) =>
            {
                if (reg.IsMatch(fullPath, SanSanSrcPath.Length + 1))
                {
                    return true;

                }
                return false;
            });
            return r;

        }
        /// <summary>
        /// 是否是VS创建的临时文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool IsVSTempFile(String fileName)
        {
            return Path.GetExtension(fileName).EndsWith("~");            
        }

        /// <summary>
        /// 文件重命名处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnRenamed(object sender, RenamedEventArgs e)
        {
            String date = DateTime.Now.ToString();
            Console.WriteLine(date + " ChangeType: {0} {1}", e.ChangeType, e.OldFullPath);
            if (isIngorePath(e.OldFullPath))
            {
                return;
                
            }
            if (Properties.Settings.Default.VSFileEditMode&&IsVSTempFile(e.OldFullPath))
            {
                //VS修改保存文件的方式是:先创建一个临时文件->再删除原文件->再把临时文件重命名为原文件
                Console.WriteLine("{0} 发现VistualStudio文件保存模式");
                copyToDistPath(e.FullPath);
            }
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
            Console.WriteLine(date + " ChangeType: {0} {1} ",  e.ChangeType, e.FullPath);
            if (isIngorePath(e.FullPath))
            {
                return;
            }
            
            
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
