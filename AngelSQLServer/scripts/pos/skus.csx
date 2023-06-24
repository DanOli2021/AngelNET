// Skus tables Ecosystem
// Skus_Catalog
// 2023-02-16
// Daniel() Oliver Rojas

public class Skus_Catalog
{
    public string id { get; set; }
    public string description { get; set; }
    public string clasification { get; set; }
    public decimal price { get; set; }
    public decimal cost { get; set; }
    public decimal consumption_tax { get; set; }
    public decimal consumption_tax1 { get; set; }
    public decimal consumption_tax2 { get; set; }
    public string currency { get; set; }
    public string maker { get; set; }
    public string location { get; set; }
    public bool requires_inventory { get; set; }
    public string image_name { get; set; }
    public bool is_for_sale { get; set; }
    public bool bulk_sale { get; set; }
    public bool require_series { get; set; }
    public bool require_lots { get; set; }
    public bool kit { get; set; }
    public bool sell_below_cost { get; set; }
    public bool locked { get; set; }
    public bool weight_request { get; set; }
    public decimal weight { get; set; }
    public string unit { get; set; }
    public string ClaveProdServ { get; set; }
    public string ClaveUnidad { get; set; }
    public string universal_id { get; set; }
}

public class Clasifications
{
    public string id { get; set; }
    public string description { get; set; }
}

public class Makers
{
    public string id { get; set; }
    public string description { get; set; }
}

public class Locations
{
    public string id { get; set; }
    public string description { get; set; }
}

public class Currencies
{
    public string id { get; set; }
    public string description { get; set; }
    public string symbol { get; set; }
    public decimal exchange_rate { get; set; }
}

public class Price_Codes
{
    public string id { get; set; }
    public string sku { get; set; }
    public string description { get; set; }
    public string currency { get; set; }
    public decimal price { get; set; }
}


public class Sku_Dictionary
{
    public string id { get; set; }
    public string sku { get; set; }
    public string description { get; set; }
    public decimal equivalence { get; set; }
}

public class Components
{
    public string id { get; set; }
    public string sku { get; set; }
    public string component_sku { get; set; }
    public decimal minimum_lot { get; set; }
    public decimal qty { get; set; }
    public string warehouse { get; set; }
    public decimal process_days { get; set; }
    public string observations { get; set; }
}
// End Skus tables Ecosystem


