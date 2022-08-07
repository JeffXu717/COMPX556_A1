using _2DRP_GUI._2DRP.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace _2DRP_GUI._2DRP
{
    class BVNSParams
    {
        public List<PlacableRect> solutionX;
        public List<int> neighbourhoods;
        public float tMax;
        public bool bestOrFirst;

        public BVNSParams(List<PlacableRect> solutionX, List<int> neighbourhoods, float tMax, bool bestOrFirst)
        {
            this.solutionX = solutionX;
            this.neighbourhoods = neighbourhoods;
            this.tMax = tMax;
            this.bestOrFirst = bestOrFirst;
        }
    }

    class BVNS2DRPSolver
    {
        public _2DRP.TwoDRPSolver _2DRPSolver;
        public delegate void TaskFinished();//声明一个在完成任务时通知主线程的委托
        public TaskFinished TaskFinishedCallBack;
        private enum NeighbourhoodType { FlipAnSegment = 1, Switch2Rects = 2 , Put2RectsFirst = 3 };
        private List<NeighbourhoodType> availableNeighbourhoods = new List<NeighbourhoodType>();
        private int currentK;
        private List<PlacableRect> currentSolution = new List<PlacableRect>();
        private List<PlacableRect> nextSolution = new List<PlacableRect>();
        private int obValNextSolution;
        private List<PlacableRect> tempSolution2 = new List<PlacableRect>();
        private List<PlacableRect> tempSolution3 = new List<PlacableRect>();
        private Random rand = new Random();

        public double ElapsedT { get; private set; }
        public int IterationCount { get; private set; }

        public BVNS2DRPSolver()
        {
            _2DRPSolver = new TwoDRPSolver();
        }

        public void BVNS(object parameters)
        {
            var bvnsParams = (BVNSParams)parameters;
            BVNS(bvnsParams.solutionX, bvnsParams.neighbourhoods, bvnsParams.tMax, bvnsParams.bestOrFirst);
        }

        public void BVNS(List<PlacableRect> solutionX, List<int> neighbourhoods, float tMax, bool bestOrFirst)
        {
            ElapsedT = 0;
            IterationCount = 0;
            availableNeighbourhoods.Clear();
            neighbourhoods.ForEach(element => availableNeighbourhoods.Add((NeighbourhoodType)element));
            int kMax = availableNeighbourhoods.Count;
            currentSolution.Clear();
            solutionX.ForEach(element => currentSolution.Add(element));
            currentSolution.Sort((x, y) => -x.CompareTo(y));   //desc sort with area
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            while (ElapsedT < tMax)
            {
                stopwatch.Restart();
                currentK = 0;
                do
                {
                    shake();
                    if (bestOrFirst)
                    {
                        bestImprovement();
                    }
                    else
                    {
                        firstImprovement();
                    }
                    neighbourhoodChange();
                    IterationCount++;
                }
                while (currentK < kMax);
                stopwatch.Stop();
                ElapsedT = ElapsedT + stopwatch.Elapsed.TotalSeconds;
            }
            _2DRPSolver.CalObjectiveVal(currentSolution);
            TaskFinishedCallBack();
        }

        private void shake()
        {
            var solutionXPrime = nextSolution;
            solutionXPrime.Clear(); // It is the container of the result
            switch (availableNeighbourhoods[currentK]) {
                case NeighbourhoodType.FlipAnSegment:
                    int leftBound = rand.Next(currentSolution.Count - 1);
                    int rightBound = rand.Next(currentSolution.Count - (leftBound + 1)) + 1 + leftBound;
                    for (int i = 0; i < currentSolution.Count; i++)
                    {
                        if (i < leftBound || i > rightBound)
                        {
                            solutionXPrime.Add(currentSolution[i]);
                        }
                        else
                        {
                            var j = rightBound + leftBound - i;
                            solutionXPrime.Add(currentSolution[j]);
                        }
                    }
                    break;
                case NeighbourhoodType.Switch2Rects:
                    int firstIndex = rand.Next(currentSolution.Count);
                    int secondIndex = rand.Next(currentSolution.Count - 1);
                    if (secondIndex >= firstIndex)
                    {
                        secondIndex = secondIndex + 1;
                    }
                    for (int i = 0; i < currentSolution.Count; i++)
                    {
                        if (i == firstIndex)
                        {
                            solutionXPrime.Add(currentSolution[secondIndex]);
                        }
                        else if (i == secondIndex)
                        {
                            solutionXPrime.Add(currentSolution[firstIndex]);
                        }
                        else
                        {
                            solutionXPrime.Add(currentSolution[i]);
                        }
                    }
                    break;
                case NeighbourhoodType.Put2RectsFirst:
                    int fIndex = rand.Next(currentSolution.Count);
                    int sIndex = rand.Next(currentSolution.Count - 1);
                    if (sIndex >= fIndex)
                    {
                        sIndex = sIndex + 1;
                    }
                    solutionXPrime.Add(currentSolution[fIndex]);
                    solutionXPrime.Add(currentSolution[sIndex]);
                    for (int i = 0; i < currentSolution.Count; i++)
                    {
                        if (i != fIndex && i != sIndex)
                        {
                            solutionXPrime.Add(currentSolution[i]);
                        }
                    }
                    break;
            }
        }

        private void bestImprovement()
        {
            var solutionX = nextSolution;
            var solutionXPrime = tempSolution2;
            var solutionTemp = tempSolution3;
            int objectiveValX = int.MaxValue;
            int objectiveValXPrime = int.MaxValue;
            do
            {
                solutionXPrime.Clear();
                solutionX.ForEach(element => solutionXPrime.Add(element));
                objectiveValXPrime = _2DRPSolver.CalObjectiveVal(solutionXPrime);
                argminObjectiveVal(solutionXPrime, solutionX, solutionTemp, out objectiveValX);　//search neighbourhood of x' and find on to fill x
            }
            while (objectiveValX < objectiveValXPrime);
            obValNextSolution = objectiveValX;
        }

        private void firstImprovement()
        {
            var solutionX = nextSolution;
            var solutionXPrime = tempSolution2;
            var solutionTemp = tempSolution3;
            int objectiveValX = int.MaxValue;
            int objectiveValXPrime = int.MaxValue;
            do
            {
                solutionXPrime.Clear();
                solutionX.ForEach(element => solutionXPrime.Add(element));
                objectiveValXPrime = _2DRPSolver.CalObjectiveVal(solutionXPrime);
                argminFirstObjectiveVal(solutionXPrime, objectiveValXPrime, solutionX, solutionTemp, out objectiveValX); //search neighbourhood of x' and find on to fill x
            }
            while (objectiveValX < objectiveValXPrime);
            obValNextSolution = objectiveValX;
        }

        private void argminFirstObjectiveVal(List<PlacableRect> solutionXPrime, int objectiveValXPrime, List<PlacableRect> solutionX, List<PlacableRect> solutionTemp, out int objectiveValX)
        {
            objectiveValX = objectiveValXPrime;
            switch (availableNeighbourhoods[currentK])
            {
                case NeighbourhoodType.FlipAnSegment:
                    for (int lB = 0; lB < solutionXPrime.Count; lB++)
                    {
                        for (int rB = lB; rB < solutionXPrime.Count; rB++)
                        {
                            solutionTemp.Clear();
                            for (int i = 0; i < solutionXPrime.Count; i++)
                            {
                                if (i < lB || i > rB)
                                {
                                    solutionTemp.Add(solutionXPrime[i]);
                                }
                                else
                                {
                                    var j = rB + lB - i;
                                    solutionTemp.Add(solutionXPrime[j]);
                                }
                            }
                            var objectiveValTemp = _2DRPSolver.CalObjectiveVal(solutionTemp);
                            if (objectiveValTemp < objectiveValX)
                            {
                                solutionX.Clear();
                                solutionTemp.ForEach(element => solutionX.Add(element));
                                objectiveValX = objectiveValTemp;
                                return;
                            }
                        }
                    }
                    break;
                case NeighbourhoodType.Switch2Rects:
                    for (int firstIndex = 0; firstIndex < solutionXPrime.Count; firstIndex++)
                    {
                        for (int secondIndex = firstIndex; secondIndex < solutionXPrime.Count; secondIndex++)
                        {
                            solutionTemp.Clear();
                            for (int i = 0; i < solutionXPrime.Count; i++)
                            {
                                if (i == firstIndex)
                                {
                                    solutionTemp.Add(solutionXPrime[secondIndex]);
                                }
                                else if (i == secondIndex)
                                {
                                    solutionTemp.Add(solutionXPrime[firstIndex]);
                                }
                                else
                                {
                                    solutionTemp.Add(solutionXPrime[i]);
                                }
                            }
                            var objectiveValTemp = _2DRPSolver.CalObjectiveVal(solutionTemp);
                            if (objectiveValTemp < objectiveValX)
                            {
                                solutionX.Clear();
                                solutionTemp.ForEach(element => solutionX.Add(element));
                                objectiveValX = objectiveValTemp;
                                return;
                            }
                        }
                    }
                    break;
                case NeighbourhoodType.Put2RectsFirst:
                    for (int firstIndex = 0; firstIndex < solutionXPrime.Count; firstIndex++)
                    {
                        for (int secondIndex = 0; secondIndex < solutionXPrime.Count; secondIndex++)
                        {
                            if (firstIndex != secondIndex)
                            {
                                solutionTemp.Clear();
                                solutionTemp.Add(solutionXPrime[firstIndex]);
                                solutionTemp.Add(solutionXPrime[secondIndex]);
                                for (int i = 0; i < solutionXPrime.Count; i++)
                                {
                                    if (i != firstIndex && i != secondIndex)
                                    {
                                        solutionTemp.Add(solutionXPrime[i]);
                                    }
                                }
                                var objectiveValTemp = _2DRPSolver.CalObjectiveVal(solutionTemp);
                                if (objectiveValTemp < objectiveValX)
                                {
                                    solutionX.Clear();
                                    solutionTemp.ForEach(element => solutionX.Add(element));
                                    objectiveValX = objectiveValTemp;
                                    return;
                                }
                            }
                        }
                    }
                    break;
            }
        }

        private void neighbourhoodChange()
        {
            var obValCurrent = _2DRPSolver.CalObjectiveVal(currentSolution);
            if (obValNextSolution < obValCurrent)
            {
                currentSolution.Clear();
                nextSolution.ForEach(element => currentSolution.Add(element));
                currentK = 0;
            }
            else
            {
                currentK += 1;
            }
        }

        private void argminObjectiveVal(List<PlacableRect> solutionXPrime, List<PlacableRect> solutionX, List<PlacableRect> solutionTemp, out int objectiveValX)
        {
            objectiveValX = int.MaxValue;
            switch (availableNeighbourhoods[currentK])
            {
                case NeighbourhoodType.FlipAnSegment:
                    for (int lB = 0; lB < solutionXPrime.Count; lB++)
                    {
                        for (int rB = lB; rB < solutionXPrime.Count; rB++)
                        {
                            solutionTemp.Clear();
                            for (int i = 0; i < solutionXPrime.Count; i++)
                            {
                                if (i < lB || i > rB)
                                {
                                    solutionTemp.Add(solutionXPrime[i]);
                                }
                                else
                                {
                                    var j = rB + lB - i;
                                    solutionTemp.Add(solutionXPrime[j]);
                                }
                            }
                            var objectiveValTemp = _2DRPSolver.CalObjectiveVal(solutionTemp);
                            if (objectiveValTemp < objectiveValX)
                            {
                                solutionX.Clear();
                                solutionTemp.ForEach(element => solutionX.Add(element));
                                objectiveValX = objectiveValTemp;
                            }
                        }
                    }
                    break;
                case NeighbourhoodType.Switch2Rects:
                    for (int firstIndex = 0; firstIndex < solutionXPrime.Count; firstIndex++)
                    {
                        for (int secondIndex = firstIndex; secondIndex < solutionXPrime.Count; secondIndex++)
                        {
                            solutionTemp.Clear();
                            for (int i = 0; i < solutionXPrime.Count; i++)
                            {
                                if (i == firstIndex)
                                {
                                    solutionTemp.Add(solutionXPrime[secondIndex]);
                                }
                                else if (i == secondIndex)
                                {
                                    solutionTemp.Add(solutionXPrime[firstIndex]);
                                }
                                else
                                {
                                    solutionTemp.Add(solutionXPrime[i]);
                                }
                            }
                            var objectiveValTemp = _2DRPSolver.CalObjectiveVal(solutionTemp);
                            if (objectiveValTemp < objectiveValX)
                            {
                                solutionX.Clear();
                                solutionTemp.ForEach(element => solutionX.Add(element));
                                objectiveValX = objectiveValTemp;
                            }
                        }
                    }
                    break;
                case NeighbourhoodType.Put2RectsFirst:
                    for (int firstIndex = 0; firstIndex < solutionXPrime.Count; firstIndex++)
                    {
                        for (int secondIndex = 0; secondIndex < solutionXPrime.Count; secondIndex++)
                        {
                            if (firstIndex != secondIndex)
                            {
                                solutionTemp.Clear();
                                solutionTemp.Add(solutionXPrime[firstIndex]);
                                solutionTemp.Add(solutionXPrime[secondIndex]);
                                for (int i = 0; i < solutionXPrime.Count; i++)
                                {
                                    if (i != firstIndex && i != secondIndex)
                                    {
                                        solutionTemp.Add(solutionXPrime[i]);
                                    }
                                }
                                var objectiveValTemp = _2DRPSolver.CalObjectiveVal(solutionTemp);
                                if (objectiveValTemp < objectiveValX)
                                {
                                    solutionX.Clear();
                                    solutionTemp.ForEach(element => solutionX.Add(element));
                                    objectiveValX = objectiveValTemp;
                                }
                            }
                        }
                    }
                    break;
            }

        }
    }
}
