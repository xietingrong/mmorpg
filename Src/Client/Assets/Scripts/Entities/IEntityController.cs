using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entities
{
    public interface IEntityController
    {
        void PlayAnim(string name);
        void SetStandby(bool standby); 
    }
}
