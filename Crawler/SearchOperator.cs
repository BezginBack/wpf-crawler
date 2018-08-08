using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Word = Microsoft.Office.Interop.Word;

namespace Crawler
{
    class SearchOperator
    {
        private string keyPhrase;
        private FileInfo document;

        public SearchOperator(FileInfo doc, string keyPhrase)
        {
            this.document = doc;
            this.keyPhrase = keyPhrase;
        }

        public struct FileMember
        {
            public string Name { get; set; }
            public string Location { get; set; }
            public string Size { get; set; }
            public string Created { get; set; }
            public string Extension { get; set; }
        }

        public delegate void AddItemToListViewDelegate(ListView listView);
        public void AddItemToListView(ListView listView)
        {
            FileMember fm = new FileMember();
            fm.Name = document.Name;
            fm.Location = document.FullName;
            fm.Size = getLength(document.Length);
            fm.Created = document.CreationTime.ToString();
            fm.Extension = document.Extension;
            if (listView.Dispatcher.Thread == Thread.CurrentThread)
            {
                listView.Items.Add(fm);
            }
            else
            {
                listView.Dispatcher.BeginInvoke(new AddItemToListViewDelegate(AddItemToListView), new object[] { listView });
            }
        }

        private string getLength(long length)
        {
            Double kilobyte = length / 1024;
            if (kilobyte < 1024)
            {
                return Math.Floor(kilobyte) + " KB";
            }
            else
            {
                Double megabyte = kilobyte / 1024;
                if (megabyte < 1024)
                {
                    return Math.Floor(megabyte) + " MB";
                } else
                {
                    return Math.Floor(megabyte / 1024) + " GB";
                }
            }
        }

        public void Operate(ListView listView)
        {
            if (File.Exists(document.FullName))
            {
                Word.Application wordApp = new Word.Application();
                wordApp.Visible = false;
                try
                {
                    Word.Document openDoc = wordApp.Documents.Open(document.FullName);
                    if (wordApp.Selection.Find.Execute(keyPhrase))
                    {
                        AddItemToListView(listView);
                    }
                    openDoc.Close();
                }
                catch (Exception)
                {

                    
                }
                wordApp.Quit(false);
            }
        }
    }
}
