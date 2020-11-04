using SkillBridge.Message;

namespace Common.Data
{
    public class CharacterDefine
    {
        public int TID { get; set; }
        public string Name { get; set; }
        public CharacterClass Class { get; set; }
        public string Resource { get; set; }
        public string Description { get; set; }

        //基本属性
        public int Speed { get; set; }
        /// <summary>
        /// 生命
        /// </summary>
        public float MaxHp { get; set; }
        /// <summary>
        /// 法力
        /// </summary>
        public float MaxMp { get; set; }
        /// <summary>
        /// 生命成长
        /// </summary>
        public float GrowthSTR { get; set; }
        /// <summary>
        /// 智力成长
        /// </summary>
        public float GrowthINT { get; set; }
        /// <summary>
        /// 敏捷成长
        /// </summary>
        public float GrowthDEX { get; set; }
        /// <summary>
        /// 力量
        /// </summary>
        public float STR { get; set; }
        /// <summary>
        /// 智力
        /// </summary>
        public float INT { get; set; }
        /// <summary>
        /// 智力
        /// </summary>
        public float DEX { get; set; }
        /// <summary>
        /// 物理攻击
        /// </summary>
        public float AD { get; set; }
        /// <summary>
        /// 法师攻击
        /// </summary>
        public float AP { get; set; }
        /// <summary>
        /// 物理防御
        /// </summary>
        public float DEF { get; set; }
        /// <summary>
        /// 法术防御
        /// </summary>
        public float MDEF { get; set; }
        /// <summary>
        /// 攻击速度
        /// </summary>
        public float SPD{ get; set; }
        /// <summary>
        /// 暴击概率
        /// </summary>
        public float CRI { get; set; }
        /// <summary>
        /// 身高
        /// </summary>
        public float Height { get; set; }

        public string AI { get; set; }

        public int LinkId { get; set; }
    }
}
