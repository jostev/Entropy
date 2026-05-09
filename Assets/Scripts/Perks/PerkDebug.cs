using UnityEngine;
using Entropy.Perks;

public class PerkDebug : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            PerksManager.Instance?.GrantPerk("sprint_coils");

        if (Input.GetKeyDown(KeyCode.Alpha2))
            PerksManager.Instance?.GrantPerk("quick_feed");

        if (Input.GetKeyDown(KeyCode.Alpha3))
            PerksManager.Instance?.GrantPerk("moon_legs");

        if (Input.GetKeyDown(KeyCode.C))
            PerksManager.Instance?.ClearAllPerks();
    }
}
