using System.IO;
using System.Windows.Forms;
using System;
using System.Collections;
using System.Collections.Generic;

namespace KIDPlayer
{
    public partial class MainForm : Form
    {
        /// <summary>
        /// 填充目录到TreeView中
        /// </summary>
        /// <param name="tvw"></param>
        /// <param name="isSource"></param>
        private void FillDirectoryTree(TreeView tvw, string rootPath, bool isShowFile)
        {
            tvw.Nodes.Clear();


            // 用所有一级子目录填充数组，如驱动器未准备好，抛出异常
            DirectoryInfo dir = new DirectoryInfo(rootPath);   // using System.IO;
            dir.GetDirectories();

            TreeNode ndRoot = new TreeNode(rootPath);

            // 为每个根目录添加节点
            tvw.Nodes.Add(ndRoot);

            // 添加子目录节点
            // 如isShowFile==true，在TreeView中显示到文件，否则只显示到目录
            GetSubDirectoryNodes(ndRoot, ndRoot.Text, isShowFile);
               
            
        }   // FillDirectoryTree

        /// <summary>
        /// 获取目录节点下的所有子目录，并添加到目录树中。
        /// 传入的参数为此子目录的父节点，此子目录的完整路径名，以及一个bool值，表示是否获取子目录的文件
        /// </summary>
        private void GetSubDirectoryNodes(TreeNode parentNode, string fullName, bool getFileNames)
        {
            DirectoryInfo dir = new DirectoryInfo(fullName);
            DirectoryInfo[] dirSubs = dir.GetDirectories();

            // 为每个子目录添加一个子节点
            foreach (DirectoryInfo dirSub in dirSubs)
            {
                // 不显示隐藏文件夹
                if ((dirSub.Attributes & FileAttributes.Hidden) != 0)
                {
                    continue;
                }

                //MessageBox.Show(dirSub.FullName);
                /// <summary>
                /// 每个目录都有完整的路径，分割后只显示最后一个节点
                /// </summary>
                



                FileInfo[] files1 = dirSub.GetFiles("*.mp3",SearchOption.AllDirectories);
                FileInfo[] files2 = dirSub.GetFiles("*.wma",SearchOption.AllDirectories);
                FileInfo[] files = new FileInfo[files1.Length + files2.Length];
                for (int i = 0; i < files1.Length; i++)
                {
                    files[i] = files1[i];
                }
                for (int i = files1.Length; i < files1.Length + files2.Length; i++)
                {
                    files[i] = files2[i - files1.Length];
                }



                if (files.Length!=0)
                {
                    TreeNode subNode = new TreeNode(dirSub.Name);
                    parentNode.Nodes.Add(subNode);

                    // 递归调用
                    GetSubDirectoryNodes(subNode, dirSub.FullName, getFileNames);
                }


            }
            if (getFileNames) // 书中源码中，这部分在foreach内部，不正确
            {
                // 获取此节点的所有文件



                FileInfo[] files1 = dir.GetFiles("*.mp3");
                FileInfo[] files2 = dir.GetFiles("*.wma");
                FileInfo[] files = new FileInfo[files1.Length + files2.Length];
                for (int i = 0; i < files1.Length; i++ )
                {
                    files[i] = files1[i];
                }
                for (int i = files1.Length; i < files1.Length + files2.Length;i++ )
                {
                    files[i] = files2[i - files1.Length];
                }



                // 放置节点后。放置子目录中的文件。
                foreach (FileInfo file in files)
                {
                    TreeNode fileNode = new TreeNode(file.Name);
                    parentNode.Nodes.Add(fileNode);
                    string temp = fileNode.FullPath;
                }
            }
        }   // GetSubDirectoryNodes


        //遍历
        private void FindCheckedNodes(TreeNode parentNode, List<TreeNode> result)
        {
            if (parentNode == null) return;

            if (parentNode.Checked == true && File.Exists(parentNode.FullPath) )
            {
                    result.Add(parentNode);    
            }

            foreach (TreeNode tn in parentNode.Nodes)
            {
                FindCheckedNodes(tn, result);
            }

        }

    }
}