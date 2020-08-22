using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Battle
{
    public class AttributeData
    {
        public float[] Data = new float[(int)AttributeType.MAX];
        /// <summary>
        /// 最大生命
        /// </summary>
        public float MaxHp { get { return Data[(int)AttributeType.MaxHP]; } set { Data[(int)AttributeType.MaxHP] = value; } }
        /// <summary>
        /// 最大法力
        /// </summary>
        public float MaxMp { get { return Data[(int)AttributeType.MaxMP]; } set { Data[(int)AttributeType.MaxMP] = value; } }
        /// <summary>
        /// 力量
        /// </summary>
        public float STR { get { return Data[(int)AttributeType.STR]; } set { Data[(int)AttributeType.STR] = value; } }
        /// <summary>
        /// 智力
        /// </summary>
        public float INT { get { return Data[(int)AttributeType.INT]; } set { Data[(int)AttributeType.INT] = value; } }
        /// <summary>
        /// 敏捷
        /// </summary>
        public float DEX { get { return Data[(int)AttributeType.DEX]; } set { Data[(int)AttributeType.DEX] = value; } }
        /// 物理攻击
        /// </summary>
        public float AD { get { return Data[(int)AttributeType.AD]; } set { Data[(int)AttributeType.AD] = value; } }
        /// <summary>
        /// 法术防御
        /// </summary>
        public float AP { get { return Data[(int)AttributeType.AP]; } set { Data[(int)AttributeType.AP] = value; } }

        /// <summary>
        /// 物理防御
        /// </summary>
        public float DEF { get { return Data[(int)AttributeType.DEF]; } set { Data[(int)AttributeType.DEF] = value; } }

        /// <summary>
        /// 法术防御
        /// </summary>
        public float MDEF { get { return Data[(int)AttributeType.MDEF]; } set { Data[(int)AttributeType.MDEF] = value; } }

        /// <summary>
        /// 攻击速度
        /// </summary>

        public float SPD { get { return Data[(int)AttributeType.SPD]; } set { Data[(int)AttributeType.SPD] = value; } }

        /// <summary>
        /// 暴击概率
        /// </summary>
        /// 
        public float CRI { get { return Data[(int)AttributeType.CRI]; } set { Data[(int)AttributeType.CRI] = value; } }
        public void Reset()
        {
            MaxHp = 0;
            MaxMp = 0;
            STR = 0;
            INT = 0;
            DEX = 0;
            AD = 0;
            AP = 0;
            DEF = 0;
            MDEF = 0;
            SPD = 0;
            CRI = 0;
        }
    }
}
