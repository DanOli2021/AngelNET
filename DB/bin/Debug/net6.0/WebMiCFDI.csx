using System;
using System.Collections.Generic;

public class Comprobante
{
    public string Version { get; set; }
    public string Serie { get; set; }
    public string Folio { get; set; }
    public string Fecha { get; set; }
    public string Sello { get; set; }
    public string FormaPago { get; set; }
    public string NoCetificado { get; set; }
    public string Cetificado { get; set; }
    public string CondicionesDePago { get; set; }
    public string SubTotal { get; set; }
    public string Descuento { get; set; }    
    public string Moneda { get; set; }
    public string TipoCambio { get; set; }
    public string Total { get; set; }
    public string TipoDeComprobante { get; set; }
    public string Exportacion { get; set; }
    public string MetodoPago { get; set; }
    public string LugarExpedicion { get; set; }
    public string Confirmacion { get; set; }
    public Emisor Emisor { get; set; } = new Emisor();
    public Receptor Receptor { get; set; } = new Receptor();
    public List<Concepto> Conceptos { get; set; } = new List<Concepto>();
    public List<Impuesto> Impuestos { get; set; } = new List<Impuesto>();
    public List<InformacionGlobal> InformacionGlobal { get; set; } = new List<InformacionGlobal>();
    public List<CfdiRelacionado> CfdiRelacionados { get; set; } = new List<CfdiRelacionado>();
    public List<string> Complemento { get; set; } = new List<string>();
    public List<string> Addenda { get; set; } = new List<string>();

}


public class Concepto
{
    public string ClaveProdServ { get; set; }
    public string NoIdentificacion { get; set; }
    public string Cantidad { get; set; }
    public string ClaveUnidad { get; set; }
    public string Unidad { get; set; }
    public string Descripcion { get; set; }
    public string ValorUnitario { get; set; }
    public string Importe { get; set; }
    public string Descuento { get; set; }
    public string ObjetoImp { get; set; }
    public List<Impuesto> Impuestos { get; set; } = new List<Impuesto>();
    public List<Retencion> Retenciones { get; set; } = new List<Retencion>();
    public List<ACuentaTerceros> ACuentaTerceros { get; set; } = new List<ACuentaTerceros>();
    public List<InformacionAduanera> InformacionAduanera { get; set; } = new List<InformacionAduanera>();
    public List<CuentaPredial> CuentaPredial { get; set; } = new List<CuentaPredial>();
    public List<string> ComplementoConcepto { get; set; } = new List<string>();
    public Parte[] Partes { get; set; } = new Parte[0];        
}


public class Emisor
{
    public string Rfc { get; set; }
    public string Nombre { get; set; }
    public string RegimenFiscal { get; set; }
    public string FacAtrAdquirente { get; set; }
}

public class Receptor
{
    public string Rfc { get; set; }
    public string Nombre { get; set; }
    public string DomicilioFiscalReceptor { get; set; }
    public string ResidenciaFiscal { get; set; }
    public string NumRegIdTrib { get; set; }
    public string RegimenFiscalReceptor { get; set; }
    public string UsoCFDI { get; set; }
}


public class Impuesto
{
    public string TotalImpuestosRetenidos { get; set; }
    public string TotalImpuestosTrasladados { get; set; }
    public List<Retencion> Retenciones { get; set; } = new List<Retencion>();
    public List<Traslado> Traslados { get; set; } = new List<Traslado>();
}

public class Traslado
{
    public string Base { get; set; }
    public string Impuesto { get; set; }
    public string TipoFactor { get; set; }
    public string TasaOCuota { get; set; }
    public string Importe { get; set; }
}


public class Retencion
{
    public string Base { get; set; }
    public string Impuesto { get; set; }
    public string TipoFactor { get; set; }
    public string TasaOCuota { get; set; }
    public string Importe { get; set; }
}


public class InformacionGlobal
{
    public string Periodicidad { get; set; }
    public string Meses { get; set; }
    public string Año { get; set; }
    public string SubTotal { get; set; }
    public string Descuento { get; set; }
}



public class CfdiRelacionado 
{
    public string UUID { get; set; }
    public string TipoRelacion { get; set; }
 
}


public class ACuentaTerceros
{
    public string RfcACuentaTerceros { get; set; }
    public string NombreACuentaTerceros { get; set; }
    public string RegimenFiscalACuentaTerceros { get; set; }

    public string DomicilioFiscalACuentaTerceros { get; set; }
    
}


public class InformacionAduanera 
{
    public string NumeroPedimento { get; set; }
}


public class CuentaPredial
{
    public string Numero { get; set; }
}


public class Parte 
{
    public List<InformacionAduanera> InformacionAduanera { get; set; } = new List<InformacionAduanera>();
    public string ClaveProdServ { get; set; }
    public string NoIdentificacion { get; set; }
    public string Cantidad { get; set; }
    public string Unidad { get; set; }
    public string Descripcion { get; set; }
    public string ValorUnitario { get; set; }
    public string Importe { get; set; }

}


public class TimbreFiscalDigital  
{
    public string Version { get; set; }
    public string UUID { get; set; }
    public string FechaTimbrado { get; set; }
    public string SelloCFD { get; set; }
    public string NoCertificadoSAT { get; set; }
    public string SelloSAT { get; set; }
}