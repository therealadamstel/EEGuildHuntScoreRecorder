using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEGuildHuntTool
{
    class ScoreLog
    {
        public string Name { get; set; }
        public string Damage { get; set; }
        public string Attempts { get; set; }
        public string Challenge { get; set; }

        public decimal DamageNumber 
        {
            get 
            {
                if (Damage.EndsWith("M"))
                {
                    return decimal.Parse(Damage.Substring(0, Damage.Length - 1));
                }

                if (Damage.EndsWith("B"))
                {
                    return decimal.Parse(Damage.Substring(0, Damage.Length - 1)) * 1000M;
                }

                return -1M;
            }
        }

        public ScoreLog(string name, string damage, string attempts)
        {
            Name = name;
            Damage = damage;
            Attempts = attempts;
        }

        // NOTE: this will never return true twice. Ugly design but IDGAF.
        public bool IsValid()
        {
            // Damage should say "Damage: X"
            if (Damage.Replace(" ", "").StartsWith("Damage:", StringComparison.OrdinalIgnoreCase) == false)
            {
                return false;
            }
            else
            {
                Damage = Damage.Substring(Damage.IndexOf(":") + 1).Trim();

                // Damage should end in "M" or "B"
                if (Damage.EndsWith("M") == false && Damage.EndsWith("B") == false)
                {
                    // BUGFIX: sometimes B's are read as 8's
                    // If it ends in an 8, we can assume it's a B because there is no M or B
                    if (Damage.EndsWith("8") == true)
                    {
                        Damage = Damage.Substring(0, Damage.Length - 1) + "B";
                    }
                }
                else if (Damage.EndsWith("TM"))
                {
                    // BUGFIX: sometimes a 1 next to M turns into a TM
                    Damage = Damage.Substring(Damage.Length - 2) + "1M";
                }
            }

            // Challenges should say "Number of challenges: X"
            if (Attempts.Replace(" ", "").StartsWith("Numberofchallenges:", StringComparison.OrdinalIgnoreCase) == false)
            {
                return false;
            }
            else
            {
                Attempts = Attempts.Substring(Attempts.IndexOf(":") + 1).Trim();
            }

            return true;
        }
    }
}
