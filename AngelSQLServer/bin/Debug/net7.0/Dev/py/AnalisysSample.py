# If you want to use parameters from db, you need to use the following code:
class mainclass:

    def __init__(self):
        self.db = None
        self.server_db = None
        self.message = None


    def main( self, db, main_db, message):

        #Your code here
        self.db = db
        self.server_db = main_db
        self.message = message

        result = db.Prompt("SQL SERVER CONNECT Data Source=.\MYBUSINESSPOS;Initial catalog=MyBusiness20farmapronto;User Id=sa;Password=12345678;Persist Security Info=True; ALIAS main")
        
        if result.startswith("Error:"):
            return result

        result = db.Prompt( "SQL SERVER QUERY SELECT TOP 100 articulo, cantidad, precio as 'precioventa', descuento, (impuesto / 100) AS 'impuesto', preciobase, observ as 'descripcion', usufecha, usuhora  FROM partvta CONNECTION ALIAS main")

        if result.startswith("Error:"):
            return result

        db.Prompt("WRITE ")

        return result