import locale

locale.setlocale(locale.LC_ALL, "")

class SkusCatalog:
    def __init__(self):
        self.id = None
        self.description = None
        self.price = None
        self.cost = None
        self.consumption_tax = None
        self.consumption_tax1 = None
        self.consumption_tax2 = None
        self.currency = None
        self.clasificacion = None
        self.maker = None
        self.location = None
        self.requires_inventory = None
        self.image_name = None
        self.it_is_for_sale = None
        self.sale_in_bulk = None
        self.require_series = None
        self.require_lots = None
        self.its_kit = None
        self.sell_below_cost = None
        self.locked = None
        self.weight_request = None
        self.weight = None
        self.price_code = None
        self.sku_dictionary = None
        self.component = None
        self.ClaveProdServ = None
        self.ClaveUnidad = None
        self.from_cfdi = None
        self.analized = None
        self.universal_id = None
        self.deleted = None

class Clasificacions:
    def __init__(self):
        self.id = None
        self.description = None

class Makers:
    def __init__(self):
        self.id = None
        self.description = None

class Locations:
    def __init__(self):
        self.id = None
        self.description = None

class PriceCodes:
    def __init__(self):
        self.id = None
        self.description = None
        self.currency = None
        self.price = None

class Currencys:
    def __init__(self):
        self.id = None
        self.description = None
        self.symbol = None
        self.exchange_rate = None

class SkuDictionary:
    def __init__(self):
        self.id = None
        self.sku = None
        self.description = None
        self.equivalence = None

class Components:
    def __init__(self):
        self.id = None
        self.sku = None
        self.component_sku = None
        self.minimum_lot = None
        self.qty = None
        self.warehouse = None
        self.process_days = None
        self.observations = None
