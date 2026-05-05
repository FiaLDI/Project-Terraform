public static class CampaignRuntimeState
{
    public static CampaignCatalogSO CurrentCatalog { get; private set; }

    public static void SetCatalog(CampaignCatalogSO catalog)
    {
        if (catalog != null)
            CurrentCatalog = catalog;
    }
}
