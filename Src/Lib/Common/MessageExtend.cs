using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SkillBridge.Message
{

    public static class MessageExtend
    {
        public static string String(this NVector3 self)
        {
            return string.Format("({0},{1},{2})", self.X, self.Y, self.Z);
        }

        public static string String(this NEntity self)
        {
            return string.Format("({0}:pos:{1},dir:{2},spd:{3}", self.Id, self.Position.String(), self.Direction.String(), self.Speed);
        }


        public static bool Equal(this NVector3 self, NVector3 rhs)
        {
            if (rhs == null) return false;
            return self.X == rhs.X && self.Y == rhs.Y && self.Z == rhs.Z;
        }


        public static bool Equal(this NEntity self, NEntity rhs)
        {
            if (self.Id != rhs.Id) return false;
            if (self.Position == null)
            {
                if (rhs.Position != null) return false;
            }
            else
                if (!self.Position.Equal(rhs.Position)) return false;

            if (self.Direction == null)
            {
                if (rhs.Direction != null) return false;
            }
            else
                 if (!self.Direction.Equal(rhs.Direction)) return false;


            return true;
        }


        public static Vector3Int FromNVector3(this Vector3Int self,NVector3 nVector)
        {
            return new Vector3Int(nVector.X, nVector.Y, nVector.Z);
        }


        public static NVector3 FromVector3Int(this NVector3 self, Vector3Int nVector)
        {
            self.X = nVector.x;
            self.Y = nVector.y;
            self.Z = nVector.z;
            return self;
        }
    }
}
