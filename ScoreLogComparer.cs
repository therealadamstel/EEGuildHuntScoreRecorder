using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEGuildHuntTool
{
    class ScoreLogComparer : IEqualityComparer<ScoreLog>
    {
        public bool Equals(ScoreLog? x, ScoreLog? y)
        {
            if (x == null && y == null)
                return true;
            if (x == null && y != null)
                return false;
            if (x != null && y == null)
                return false;

            return x.Name == y.Name
                && x.Attempts == y.Attempts
                && x.Damage == y.Damage
                && x.Challenge == y.Challenge;
        }

        public int GetHashCode([DisallowNull] ScoreLog obj)
        {
            return (obj.Name + obj.Attempts + obj.Damage).GetHashCode();
        }
    }
}
