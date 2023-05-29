using System;

public class skus_catalog
{
    public string id { get; set; }
    public string description { get; set; }
    public decimal price { get; set; }
    public decimal cost { get; set; }
    public decimal consumption_tax { get; set; }
    public decimal consumption_tax1 { get; set; }
    public decimal consumption_tax2 { get; set; }
    public string currency { get; set; }
    public string clasificacion { get; set; }
    public string maker { get; set; }
    public string location { get; set; }
    public bool requires_inventory { get; set; }
    public string image_name { get; set; }
    public bool it_is_for_sale { get; set; }
    public bool sale_in_bulk { get; set; }
    public bool require_series { get; set; }
    public bool require_lots { get; set; }
    public bool its_kit { get; set; }
    public bool sell_below_cost { get; set; }
    public bool locked { get; set; }
    public bool weight_request { get; set; }
    public decimal weight { get; set; }
    public string price_code { get; set; }
    public string sku_dictionary { get; set; }
    public string component { get; set; }
    public string ClaveProdServ { get; set; }
    public string ClaveUnidad { get; set; }
    public bool from_cfdi { get; set; }
    public bool analized { get; set; }
    public string universal_id { get; set; }
    public bool deleted { get; set; }
}

public class Clasificacions
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

public class price_codes
{
    public string id { get; set; }
    public string description { get; set; }
    public string currency { get; set; }
    public string price { get; set; }
}


public class Currencys
{
    public string id { get; set; }
    public string description { get; set; }
    public string symbol { get; set; }
    public decimal exchange_rate { get; set; }
}

public class sku_dictionary
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

