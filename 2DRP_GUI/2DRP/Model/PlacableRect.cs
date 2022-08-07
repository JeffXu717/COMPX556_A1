using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace _2DRP_GUI._2DRP.Model
{
    struct PlacableRect : IComparable<PlacableRect>
    {
        public int id;
        public int width;
        public int height;
        public int area;

        public int x; //bl-corner
        public int y;
        public bool rotateFlag;

        public int UR_x
        {
            get
            {
                return rotateFlag ? x + height : x + width;
            }
        }

        public int UR_y
        {
            get
            {
                return rotateFlag ? y + width : y + height;
            }
        }

        public int WidthAfterRotate
        {
            get
            {
                return rotateFlag ? height : width;
            }
        }

        public int HeightAfterRotate
        {
            get
            {
                return rotateFlag ? width : height;
            }
        }

        public PlacableRect(int id, int width, int height)
        {
            this.id = id;
            this.width = width;
            this.height = height;
            this.area = width * height;
            this.x = -1;
            this.y = -1;
            this.rotateFlag = false;
        }

        public PlacableRect(PlacableRect other)
        {
            this.id = other.id;
            this.width = other.width;
            this.height = other.height;
            this.area = other.area;
            this.x = other.x;
            this.y = other.y;
            this.rotateFlag = other.rotateFlag;
        }

        public void SetCoodinates(int x, int y, bool rotateFlag)
        {
            this.x = x;
            this.y = y;
            this.rotateFlag = rotateFlag;
        }

        public int CompareTo([AllowNull] PlacableRect other)
        {
            if (this.area > other.area)
                return 1;
            else
                return -1;
        }

        public bool IfIntersectWith(Rect other)
        {
            // notice that placable rect can be rotated
            float halfWidth = rotateFlag ? (float)height / 2 : (float)width / 2;
            float halfHeight = rotateFlag ? (float)width /2 : (float)height / 2;
            float centerX = x + halfWidth;
            float centerY = y + halfHeight;
            float otherHalfWidth = (float)other.width / 2;
            float otherCenterX = other.x + otherHalfWidth;
            if (other.height != int.MaxValue)
            {
                float otherHalfHeight = (float)other.height / 2;
                float otherCenterY = other.y + otherHalfHeight;
                return (Math.Abs(centerX - otherCenterX) < halfWidth + otherHalfWidth) && (Math.Abs(centerY - otherCenterY) < halfHeight + otherHalfHeight);
            }
            else
            {
                return (Math.Abs(centerX - otherCenterX) < halfWidth + otherHalfWidth) && other.y < UR_y;
            }
        }

        public override string ToString()
        {
            return id.ToString().PadRight(7) + ("w:" + width).PadRight(8) + "h:" + height;
        }
    }

    struct Rect
    {
        public int width;
        public int height; // if height is equal to int.MaxValue, it means the height is Inf Max

        public int x; //bl-corner
        public int y;

        public int UR_x
        {
            get
            {
                return x + width;
            }
        }

        public int UR_y
        {
            get
            {
                return height == int.MaxValue ? int.MaxValue : y + height;
            }
        }


        public Rect(int x, int y, int width, int height)
        {
            this.width = width;
            this.height = height;
            this.x = x;
            this.y = y;
        }

        public void SetCoodinates(Rect rect)
        {
            SetCoodinates(rect.x, rect.y);
        }

        public void SetCoodinates(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public void SetSize(Rect rect)
        {
            SetSize(rect.width, rect.height);
        }

        public void SetSize(int w, int h)
        {
            this.width = w;
            this.height = h;
        }

        public bool ContainedByOther(Rect other)
        {
            return x >= other.x && y >= other.y && UR_x <= other.UR_x && UR_y <= other.UR_y;
        }
        public override string ToString()
        {
            return ("x:" + x).PadRight(5) + ("y:" + y).PadRight(5) + ("w:" + width).PadRight(5) + "h:" + height;
        }
    }
}
