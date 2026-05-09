using System.Collections;
using UnityEngine;

namespace Entropy.Perks
{
    /// <summary>
    /// Complex perk with event hooks and optional coroutine effects.
    /// </summary>
    public abstract class AdvancedPerk : PerkBase
    {
        public virtual void OnActionTriggered(ActionEvent evt) { }
        public virtual IEnumerator CustomEffect() { yield break; }
    }
}
