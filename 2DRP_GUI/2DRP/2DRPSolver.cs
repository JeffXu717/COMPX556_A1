using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using _2DRP_GUI._2DRP.Model;

namespace _2DRP_GUI._2DRP
{
    class TwoDRPSolver
    {
        public List<PlacableRect> placedRects = new List<PlacableRect>();
        public int heightOfPlacedRects;
        public int W = 100;
        List<PlacableRect> unplacedRects = new List<PlacableRect>();
        List<Rect> emptyRectSpaces = new List<Rect>();

        //temp data
        List<Rect> tempEmptyRects = new List<Rect>();
        List<int> tempIndex = new List<int>();

        public double ElapsedT { get; private set; }

        public delegate void TaskFinished();//声明一个在完成任务时通知主线程的委托
        public TaskFinished TaskFinishedCallBack;

        public void _2DRPSolver(object rectList)
        {

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            unplacedRects.Clear();
            placedRects.Clear();
            emptyRectSpaces.Clear();
            heightOfPlacedRects = 0;

            emptyRectSpaces.Add(new Rect(0, 0, W, int.MaxValue));
            unplacedRects = ((List<PlacableRect>)rectList).Where(rect => (rect.width <= W || rect.height <= W)).ToList(); //filtrate unplacable rects
            unplacedRects.Sort((x, y) => -x.CompareTo(y));   //desc sort with area

            MaxRects();

            calHeightOfPlacedRects();

            stopwatch.Stop();
            ElapsedT = stopwatch.Elapsed.TotalSeconds;

            TaskFinishedCallBack();
        }

        public int CalObjectiveVal(List<PlacableRect> solution)
        {
            unplacedRects.Clear();
            placedRects.Clear();
            emptyRectSpaces.Clear();
            heightOfPlacedRects = 0;

            emptyRectSpaces.Add(new Rect(0, 0, W, int.MaxValue));
            unplacedRects = solution.Where(rect => (rect.width <= W || rect.height <= W)).ToList(); //filtrate unplacable rects
            //do not need sort

            MaxRects();

            calHeightOfPlacedRects();

            return heightOfPlacedRects;
        }

        private void calHeightOfPlacedRects()
        {

            foreach (var placedRect in placedRects)
            {
                if (placedRect.UR_y > heightOfPlacedRects)
                {
                    heightOfPlacedRects = placedRect.UR_y;
                }
            }
        }

        #region MaxRects
        public void MaxRects() 
        {
            foreach (var unPlacedRect in unplacedRects)
            {
                bool soFar_rotateFlag;
                int soFar_index;
                //searchForBestFitSpace(out soFar_rotateFlag, out soFar_index, unPlacedRect);
                searchForTheLowestTopSideY(out soFar_rotateFlag, out soFar_index, unPlacedRect);

                //place Rect
                var placedRect = new PlacableRect(unPlacedRect);
                placedRect.SetCoodinates(emptyRectSpaces[soFar_index].x, emptyRectSpaces[soFar_index].y, soFar_rotateFlag);
                placedRects.Add(placedRect);

                //Use the MAXRECTS split scheme to subdivide emptyRectSpace into 2 maximal rect spaces, or 1 maximal rect space, or zero.
                createNewEmptySpaces(placedRect, emptyRectSpaces[soFar_index], soFar_index);

                detectIntersecting(placedRect);

                clearNoMaximalEmptySpace();
            }
        }

        private void clearNoMaximalEmptySpace()
        {
            tempIndex.Clear();
            for (int i = 0; i < emptyRectSpaces.Count; i++)
            {
                var emptySpace = emptyRectSpaces[i];
                for (int j = 0; j < emptyRectSpaces.Count; j++)
                {
                    var tempEmptySpace = emptyRectSpaces[j];
                    if (i != j && emptySpace.ContainedByOther(tempEmptySpace))
                    {
                        tempIndex.Add(i);
                        break;
                    }
                }
            }

            for (int i = tempIndex.Count - 1; i >= 0; i--)
            {
                emptyRectSpaces.RemoveAt(tempIndex[i]);
            }
        }

        private void detectIntersecting(PlacableRect placedRect)
        {
            tempEmptyRects.Clear();
            tempIndex.Clear();
            for (int i = 0; i < emptyRectSpaces.Count; i++)
            {
                var emptySpace = emptyRectSpaces[i];
                if (placedRect.IfIntersectWith(emptySpace)) // intersect with certain emptySpace
                {
                    tempIndex.Add(i);
                    var tempRect = new Rect();
                    //left side
                    if (placedRect.x > emptySpace.x)
                    {
                        tempRect.SetCoodinates(emptySpace);
                        tempRect.SetSize(placedRect.x - emptySpace.x, emptySpace.height);
                        tempEmptyRects.Add(tempRect);
                    }

                    //right side
                    if (emptySpace.UR_x > placedRect.UR_x)
                    {
                        tempRect.SetCoodinates(placedRect.UR_x, emptySpace.y);
                        tempRect.SetSize(emptySpace.UR_x - placedRect.UR_x, emptySpace.height);
                        tempEmptyRects.Add(tempRect);
                    }

                    //upper side
                    if (emptySpace.UR_y > placedRect.UR_y)
                    {
                        tempRect.SetCoodinates(emptySpace.x, placedRect.UR_y);
                        tempRect.SetSize(emptySpace.width, (emptySpace.height == int.MaxValue ? int.MaxValue : emptySpace.UR_y - placedRect.UR_y));
                        tempEmptyRects.Add(tempRect);
                    }

                    //bottom side
                    if (placedRect.y > emptySpace.y)
                    {
                        tempRect.SetCoodinates(emptySpace);
                        tempRect.SetSize(emptySpace.width, placedRect.y - emptySpace.y);
                        tempEmptyRects.Add(tempRect);
                    }
                }
            }

            for (int i = tempIndex.Count - 1; i >= 0; i--)
            {
                emptyRectSpaces.RemoveAt(tempIndex[i]);
            }

            foreach (var rect in tempEmptyRects)
            {
                emptyRectSpaces.Add(rect);
            }
        }

        private void searchForTheLowestTopSideY(out bool soFar_rotateFlag, out int soFar_index, PlacableRect unPlacedRect)
        {
            soFar_rotateFlag = false;
            soFar_index = -1;
            var soFar_y_topSide = int.MaxValue;
            var bSSDSoFar = int.MaxValue; 
            for (int i = 0; i < emptyRectSpaces.Count; i++)
            {
                var emptySpace = emptyRectSpaces[i];
                int oY_topSide = int.MaxValue;
                int rY_topSide = int.MaxValue;
                if (emptySpace.width >= unPlacedRect.width && emptySpace.height >= unPlacedRect.height)
                {
                    oY_topSide = calTopSideY(unPlacedRect, emptySpace, false);
                }
                if (emptySpace.width >= unPlacedRect.height && emptySpace.height >= unPlacedRect.width)
                {
                    rY_topSide = calTopSideY(unPlacedRect, emptySpace, true);
                }

                int oBSSD = int.MaxValue; // original Best Short Side difference
                int rBSSD = int.MaxValue; // rotated Best Short Side difference
                if (emptySpace.width >= unPlacedRect.width && emptySpace.height >= unPlacedRect.height)
                {
                    var heightDiff = emptySpace.height == int.MaxValue ? int.MaxValue : emptySpace.height - unPlacedRect.height;
                    oBSSD = Math.Min(emptySpace.width - unPlacedRect.width, heightDiff);
                }
                if (emptySpace.width >= unPlacedRect.height && emptySpace.height >= unPlacedRect.width)
                {
                    var heightDiff = emptySpace.height == int.MaxValue ? int.MaxValue : emptySpace.height - unPlacedRect.width;
                    rBSSD = Math.Min(emptySpace.width - unPlacedRect.height, heightDiff);
                }

                int minY_topSide_in2Direction = Math.Min(oY_topSide, rY_topSide);
                if (minY_topSide_in2Direction == int.MaxValue) // In two directions it can not placed
                {
                    continue;
                }
                else if (minY_topSide_in2Direction == oY_topSide) // qualified in original direction
                {
                    fillTheLowestTopSideYSoFarParameters(ref bSSDSoFar, ref soFar_y_topSide, ref soFar_rotateFlag, ref soFar_index,
                        oBSSD, oY_topSide, false, i);
                }
                else if (minY_topSide_in2Direction == rY_topSide) // qualified in rotated direction
                {
                    fillTheLowestTopSideYSoFarParameters(ref bSSDSoFar, ref soFar_y_topSide, ref soFar_rotateFlag, ref soFar_index,
                        rBSSD, rY_topSide, true, i);
                }
            }
        }

        private void fillTheLowestTopSideYSoFarParameters(ref int bSSDSoFar, ref int soFar_y_upperSide, ref bool soFar_rotateFlag, ref int soFar_index,
            int bSSD, int y_upperSide, bool rotateFlag, int index)
        {
            if (y_upperSide < soFar_y_upperSide ||
                ( y_upperSide == soFar_y_upperSide && bSSD < bSSDSoFar)) // best short side diff with the first priority
            {
                bSSDSoFar = bSSD;
                soFar_rotateFlag = rotateFlag;
                soFar_y_upperSide = y_upperSide;
                soFar_index = index;
            }
        }

        #region BSSF
        private void searchForBestSSFitSpace(out bool bSSDSoFar_rotateFlag, out int bSSDSoFar_index, PlacableRect unPlacedRect)
        {
            var bSSDSoFar = int.MaxValue; // Best Short Side difference so far
            var bSSDSoFar_y_topSide = int.MaxValue;
            bSSDSoFar_rotateFlag = false;
            bSSDSoFar_index = -1;
            for (int i = 0; i < emptyRectSpaces.Count; i++)
            {
                var emptySpace = emptyRectSpaces[i];
                int oBSSD = int.MaxValue; // original Best Short Side difference
                int rBSSD = int.MaxValue; // rotated Best Short Side difference
                if (emptySpace.width >= unPlacedRect.width && emptySpace.height >= unPlacedRect.height)
                {
                    var heightDiff = emptySpace.height == int.MaxValue ? int.MaxValue : emptySpace.height - unPlacedRect.height;
                    oBSSD = Math.Min(emptySpace.width - unPlacedRect.width, heightDiff);
                }
                if (emptySpace.width >= unPlacedRect.height && emptySpace.height >= unPlacedRect.width)
                {
                    var heightDiff = emptySpace.height == int.MaxValue ? int.MaxValue : emptySpace.height - unPlacedRect.width;
                    rBSSD = Math.Min(emptySpace.width - unPlacedRect.height, heightDiff);
                }

                int bSSD_in2Direction = Math.Min(oBSSD, rBSSD);
                if (bSSD_in2Direction == int.MaxValue) // In two directions it can not placed
                {
                    continue;
                }
                else if (oBSSD == rBSSD) //oBSSD == rBSSD but is not equal to int.MaxValue
                {
                    var oBSSD_y_topSide = calTopSideY(unPlacedRect, emptySpace, false);
                    var rBSSD_y_topSide = calTopSideY(unPlacedRect, emptySpace, true);
                    var bSSD_in2Direction_y_upperSide = Math.Min(oBSSD_y_topSide, rBSSD_y_topSide);
                    var bSSD_in2Direction_rotateFlag = oBSSD_y_topSide == bSSD_in2Direction_y_upperSide ? false : true;
                    fillBSSDSoFarParameters(ref bSSDSoFar, ref bSSDSoFar_y_topSide, ref bSSDSoFar_rotateFlag, ref bSSDSoFar_index,
                    bSSD_in2Direction, bSSD_in2Direction_y_upperSide, bSSD_in2Direction_rotateFlag, i);
                }
                else if (bSSD_in2Direction == oBSSD) // qualified in original direction
                {
                    var oBSSD_y_topSide = calTopSideY(unPlacedRect, emptySpace, false);
                    fillBSSDSoFarParameters(ref bSSDSoFar, ref bSSDSoFar_y_topSide, ref bSSDSoFar_rotateFlag, ref bSSDSoFar_index,
                    oBSSD, oBSSD_y_topSide, false, i);
                }
                else if (bSSD_in2Direction == rBSSD) // qualified in rotated direction
                {
                    var rBSSD_y_topSide = calTopSideY(unPlacedRect, emptySpace, true);
                    fillBSSDSoFarParameters(ref bSSDSoFar, ref bSSDSoFar_y_topSide, ref bSSDSoFar_rotateFlag, ref bSSDSoFar_index,
                    rBSSD, rBSSD_y_topSide, true, i);

                }
            }

        }

        private int calTopSideY(PlacableRect placableRect, Rect emptySpace, bool rotateFlag)
        {
            return rotateFlag ? emptySpace.y + placableRect.width : emptySpace.y + placableRect.height;
        }

        private void fillBSSDSoFarParameters(ref int bSSDSoFar, ref int bSSDSoFar_y_upperSide, ref bool bSSDSoFar_rotateFlag, ref int bSSDSoFar_index,
            int bSSD, int bSSD_y_upperSide, bool bSSD_rotateFlag, int bSSD_index)
        {
            if (bSSD < bSSDSoFar || 
                (bSSD == bSSDSoFar && bSSD_y_upperSide < bSSDSoFar_y_upperSide)) // best short side diff with the first priority
            {
                bSSDSoFar = bSSD;
                bSSDSoFar_rotateFlag = bSSD_rotateFlag;
                bSSDSoFar_y_upperSide = bSSD_y_upperSide;
                bSSDSoFar_index = bSSD_index;
            }
        }
        #endregion

        private void createNewEmptySpaces(PlacableRect placedRect, Rect emptySpace, int emptySpaceIndex)
        {
            // Space out
            emptyRectSpaces.RemoveAt(emptySpaceIndex);

            var placedRectActualWidth = placedRect.rotateFlag ? placedRect.height : placedRect.width;
            var placedRectActualHeight = placedRect.rotateFlag ? placedRect.width : placedRect.height;

            if (emptySpace.width - placedRectActualWidth > 0)
            {
                Rect subEmptySpace1 = new Rect();
                subEmptySpace1.x = emptySpace.x + placedRectActualWidth;
                subEmptySpace1.y = emptySpace.y;
                subEmptySpace1.width = emptySpace.width - placedRectActualWidth;
                subEmptySpace1.height = emptySpace.height;
                emptyRectSpaces.Add(subEmptySpace1);
            }

            if ((emptySpace.height == int.MaxValue ? int.MaxValue : emptySpace.height - placedRectActualHeight) > 0)
            {
                Rect subEmptySpace2 = new Rect();
                subEmptySpace2.x = emptySpace.x;
                subEmptySpace2.y = emptySpace.y + placedRectActualHeight;
                subEmptySpace2.width = emptySpace.width;
                subEmptySpace2.height = emptySpace.height == int.MaxValue ? int.MaxValue : emptySpace.height - placedRectActualHeight;
                emptyRectSpaces.Add(subEmptySpace2);
            }
        }
        #endregion

    }
}
