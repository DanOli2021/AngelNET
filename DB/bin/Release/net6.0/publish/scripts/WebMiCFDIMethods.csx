#load "WebMiCFDI.csx"
#r "D:/My/DB/DB/bin/Debug/net6.0/DB.dll"
#r "D:/My/DB/DB/bin/Debug/net6.0/Newtonsoft.Json.dll"

using System.Text;
using OpenSSL;
using Newtonsoft.Json;

Comprobante comprobante = new Comprobante();
comprobante.Version = "4.0";
comprobante.Serie = "A";
comprobante.Folio = "1";
comprobante.Fecha = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
comprobante.FormaPago = "06";
comprobante.CondicionesDePago = "Contado";
comprobante.SubTotal = "1";
comprobante.Moneda = "MXN";
comprobante.Total = "1.16";
comprobante.TipoDeComprobante = "I";
comprobante.MetodoPago = "PUE";
comprobante.LugarExpedicion = "52156";
comprobante.Emisor.Rfc = "MPD0611012U4";
comprobante.Emisor.Nombre = "\"MYBUSINESS POS DESARROLLOS\"";
comprobante.Emisor.RegimenFiscal = "601";
comprobante.Receptor.Rfc = "XAXX010101000";
comprobante.Receptor.Nombre = "DANIEL OLIVER ROJAS ESCOBAR";
comprobante.Receptor.UsoCFDI = "S01";
comprobante.Receptor.RegimenFiscalReceptor = "616";
comprobante.Receptor.DomicilioFiscalReceptor = "52156";
comprobante.Exportacion = "01";

Concepto concepto = new Concepto();
concepto.ClaveProdServ = "84111506";
concepto.NoIdentificacion = "TIMBRES";
concepto.Cantidad = "1";
concepto.ClaveUnidad = "E48";
concepto.Unidad = "NA";
concepto.Descripcion = "TIMBRES";
concepto.ValorUnitario = "1";
concepto.Importe = "1";
concepto.ObjetoImp = "02";
Impuesto impuesto_concepto = new Impuesto();

Traslado traslado = new Traslado();
traslado.Base = "1.000000";
traslado.Impuesto = "002";
traslado.TipoFactor = "Tasa";
traslado.TasaOCuota = "0.160000";
traslado.Importe = "0.16";

//<cfdi:Traslado Base="322" Impuesto="002" TipoFactor="Tasa" TasaOCuota="0.000000" Importe="0.00" />

impuesto_concepto.Traslados.Add(traslado);
concepto.Impuestos.Add( impuesto_concepto );

Impuesto impuesto_comprobante = new Impuesto();
impuesto_comprobante.TotalImpuestosTrasladados = "0.16";

Traslado traslado_comprobante = new Traslado();
traslado_comprobante.Base = "1.000000";
traslado_comprobante.Impuesto = "002";
traslado_comprobante.TipoFactor = "Tasa";
traslado_comprobante.TasaOCuota = "0.160000";
traslado_comprobante.Importe = "0.16";

impuesto_comprobante.TotalImpuestosTrasladados = "0.16";
impuesto_comprobante.Traslados.Add(traslado_comprobante);
comprobante.Impuestos.Add(impuesto_comprobante);

comprobante.Conceptos.Add( concepto );

string certificate = "";
string certificate_number = "";
opensslkey.CertificateData( @"D:/Daniel/CSD/00001000000507298690.cer", out certificate, out certificate_number);
comprobante.NoCetificado = certificate_number;
comprobante.Cetificado = certificate;

string cadena_orginal = CFDITools.CreateCadenaOriginal(comprobante);
string signed_string = opensslkey.SignString(@"D:/Daniel/CSD/CSD_MYBUSINESS_POS_DESARROLLOS_SA_DE_CV_MPD0611012U4_20210504_184011.key", "MBPDRR6805", cadena_orginal);
comprobante.Sello = signed_string;

//return opensslkey.VerifyFirm( cadena_orginal, comprobante.Sello, comprobante.Cetificado);

return JsonConvert.SerializeObject(comprobante, Formatting.Indented);

static public class CFDITools
{
    public static string CreateCadenaOriginal(Comprobante cfd)
    {
        try
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("||");
            AddValue(sb, cfd.Version);
            AddValue(sb, cfd.Serie);
            AddValue(sb, cfd.Folio);
            AddValue(sb, cfd.Fecha);
            AddValue(sb, cfd.FormaPago);
            AddValue(sb, cfd.NoCetificado);
            AddValue(sb, cfd.CondicionesDePago);
            AddValue(sb, cfd.SubTotal);
            AddValue(sb, cfd.Descuento);
            AddValue(sb, cfd.Moneda);
            AddValue(sb, cfd.TipoCambio);
            AddValue(sb, cfd.Total);
            AddValue(sb, cfd.TipoDeComprobante);
            AddValue(sb, cfd.Exportacion);
            AddValue(sb, cfd.MetodoPago);
            AddValue(sb, cfd.LugarExpedicion);
            AddValue(sb, cfd.Confirmacion);

            foreach (InformacionGlobal item in cfd.InformacionGlobal)
            {
                AddValue(sb, item.Periodicidad);
                AddValue(sb, item.Meses);
                AddValue(sb, item.Año);
            }

            foreach (CfdiRelacionado item in cfd.CfdiRelacionados)
            {
                AddValue(sb, item.TipoRelacion);
                AddValue(sb, item.UUID);
            }

            AddValue(sb, cfd.Emisor.Rfc);
            AddValue(sb, cfd.Emisor.Nombre);
            AddValue(sb, cfd.Emisor.RegimenFiscal);
            AddValue(sb, cfd.Emisor.FacAtrAdquirente);
            AddValue(sb, cfd.Receptor.Rfc);
            AddValue(sb, cfd.Receptor.Nombre);
            AddValue(sb, cfd.Receptor.DomicilioFiscalReceptor);
            AddValue(sb, cfd.Receptor.ResidenciaFiscal);
            AddValue(sb, cfd.Receptor.NumRegIdTrib);
            AddValue(sb, cfd.Receptor.RegimenFiscalReceptor);
            AddValue(sb, cfd.Receptor.UsoCFDI);

            foreach (Concepto concepto in cfd.Conceptos)
            {
                AddValue(sb, concepto.ClaveProdServ);
                AddValue(sb, concepto.NoIdentificacion);
                AddValue(sb, concepto.Cantidad);
                AddValue(sb, concepto.ClaveUnidad);
                AddValue(sb, concepto.Unidad);
                AddValue(sb, concepto.Descripcion);
                AddValue(sb, concepto.ValorUnitario);
                AddValue(sb, concepto.Importe);
                AddValue(sb, concepto.Descuento);
                AddValue(sb, concepto.ObjetoImp);
                //AddValue(sb, concepto.ObjetoImp);

                foreach (Impuesto impuesto in concepto.Impuestos)
                {
                    foreach (Traslado traslado in impuesto.Traslados)
                    {
                        AddValue(sb, traslado.Base);
                        AddValue(sb, traslado.Impuesto);
                        AddValue(sb, traslado.TipoFactor);
                        AddValue(sb, traslado.TasaOCuota);
                        AddValue(sb, traslado.Importe);
                    }

                    foreach (Retencion retencion in impuesto.Retenciones)
                    {
                        AddValue(sb, retencion.Base);
                        AddValue(sb, retencion.Impuesto);
                        AddValue(sb, retencion.TipoFactor);
                        AddValue(sb, retencion.TasaOCuota);
                        AddValue(sb, retencion.Importe);
                    }

                    foreach (ACuentaTerceros acuentaTerceros in concepto.ACuentaTerceros)
                    {
                        AddValue(sb, acuentaTerceros.RfcACuentaTerceros);
                        AddValue(sb, acuentaTerceros.NombreACuentaTerceros);
                        AddValue(sb, acuentaTerceros.RegimenFiscalACuentaTerceros);
                        AddValue(sb, acuentaTerceros.DomicilioFiscalACuentaTerceros);
                    }

                    foreach (InformacionAduanera informacionAduanera in concepto.InformacionAduanera)
                    {
                        AddValue(sb, informacionAduanera.NumeroPedimento);
                    }

                    foreach (Parte parte in concepto.Partes)
                    {
                        AddValue(sb, parte.ClaveProdServ);
                        AddValue(sb, parte.NoIdentificacion);
                        AddValue(sb, parte.Cantidad);
                        AddValue(sb, parte.Unidad);
                        AddValue(sb, parte.Descripcion);
                        AddValue(sb, parte.ValorUnitario);
                        AddValue(sb, parte.Importe);

                        foreach (InformacionAduanera informacionAduanera in parte.InformacionAduanera)
                        {
                            AddValue(sb, informacionAduanera.NumeroPedimento);
                        }
                        
                    }
                }
            }

            foreach (Impuesto impuesto in cfd.Impuestos)
            {

                foreach (Retencion retencion in impuesto.Retenciones)
                {
                    AddValue(sb, retencion.Impuesto);
                    AddValue(sb, retencion.Importe);
                }

                AddValue( sb, impuesto.TotalImpuestosRetenidos );

                foreach (Traslado traslado in impuesto.Traslados)
                {
                    AddValue(sb, traslado.Base);
                    AddValue(sb, traslado.Impuesto);
                    AddValue(sb, traslado.TipoFactor);
                    AddValue(sb, traslado.TasaOCuota);
                    AddValue(sb, traslado.Importe);
                }

                AddValue( sb, impuesto.TotalImpuestosTrasladados );

            } 

            sb.Append("|");

            return sb.ToString();

        }
        catch (System.Exception e)
        {
            return $"Error: CreateCadenaOriginal {e}";
        }

    }


    public static void AddValue(StringBuilder sb, string value)
    {

        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        sb.Append(value.Trim());
        sb.Append("|");
    }

}