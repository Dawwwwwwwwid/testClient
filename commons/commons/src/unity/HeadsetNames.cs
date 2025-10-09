using System.Collections.Generic;

/// <summary>
/// HeadsetNames contains a dictionary to match the AltHmdRadioSocket IDs to their respective headset names.
/// </summary>
public static class HeadsetNames
{
    private static ulong thisHeadset = 0;
    private static readonly Dictionary<ulong, string> headsetNames = new Dictionary<ulong, string>()
    {
        {12877285084921191104, "CS_VR_01" },
        {16037103357798992572, "CS_VR_02" },
        {3828154284960194432, "CS_VR_03" },
        {2786681269143656785, "CS_VR_06" },
        {11292603482698740943, "CS_VR_09" },
        {18330951449666787893, "CS_VR_12" },
    };
    

    /// <summary>
    /// Try get a headset name if the given ID is found.
    /// </summary>
    /// <param name="headsetID">ID to check the dictionary for.</param>
    /// <param name="headsetName">Headsetname if it is found.</param>
    /// <returns>Returns a Boolean indicating if the headset name was found for the given ID.</returns>
    public static  bool TryGetHeadsetName(ulong headsetID, out string headsetName)
    {
        return headsetNames.TryGetValue(headsetID, out headsetName);
    }

    /// <summary>
    /// Checks a given ID against the ID on the headset running the code.
    /// </summary>
    /// <param name="headsetID">ID to check against.</param>
    /// <returns>Returns a Boolean indicating if the IDs match.</returns>
    public static bool IsThisHeadset(ulong headsetID)
    {
        return headsetID == thisHeadset;
    }

    /// <summary>
    /// Sets the headset ID value to the given ID.
    /// </summary>
    /// <param name="headsetID">The given ID to set the headset ID to.</param>
    public static void SetThisHeadset(ulong headsetID)
    {
        thisHeadset = headsetID;
    }
}
