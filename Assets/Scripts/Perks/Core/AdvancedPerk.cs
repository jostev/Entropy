using System.Collections;
using UnityEngine;

namespace Entropy.Perks
{
    public abstract class AdvancedPerk : PerkBase
    {
        public virtual void OnActionTriggered(ActionEvent evt) { }
        public virtual IEnumerator CustomEffect() { yield break; }
    }
}
