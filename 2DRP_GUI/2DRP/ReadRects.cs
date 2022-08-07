using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using _2DRP_GUI._2DRP.Model;
using _2DRP_GUI.Utility;

namespace _2DRP_GUI._2DRP
{
    class ReadRects
    {
        public List<PlacableRect> rects { get { return _rects; } }
        private List<PlacableRect> _rects = new List<PlacableRect>();

        public int TotalArea { get; private set; }

        //public delegate void UpdateUI(int step);//声明一个更新主线程的委托
        //public UpdateUI UpdateUIDelegate;

        public delegate void TaskFinished();//声明一个在完成任务时通知主线程的委托
        public TaskFinished TaskFinishedCallBack;

        public void Read(object filepath)
        {
            //clear data in memory
            _rects.Clear();
            TotalArea = 0;

            var lineList = CSVReader.OpenCSV((string)filepath);
            stringList2RectList(lineList);
            //任务完成时通知主线程作出相应的处理
            TaskFinishedCallBack();
        }

        public void stringList2RectList(List<string> lineList)
        {
            string[] tableLine = null;
            int i;
            for (i = 0; i < lineList.Count - 1; i++)
            {
                tableLine = lineList[i].Split(',');
                _rects.Add(new PlacableRect(int.Parse(tableLine[0]), int.Parse(tableLine[1]), int.Parse(tableLine[2])));
            }
            TotalArea = int.Parse(lineList[i]);
        }
    }
}
