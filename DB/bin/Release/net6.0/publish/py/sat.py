import json
import locale

locale.setlocale( locale.LC_ALL, "" )

class mainclass:

    def __init__( self, db, data ):
        self.db = db
        self.data = data
        self.parameters = json.loads(self.data)

    def get_form(self):
        
        self.db.Prompt( "WEB FORM CLEAR" ) 

        self.db.Prompt( """WEB FORM CONTROL db1
                           TYPE db 
                           VALUE "DB USER db PASSWORD db ACCOUNT sat DATABASE productossat DATA DIRECTORY d:\db\sat"
                           """  ) 

        self.db.Prompt( """WEB FORM CONTROL run1
                           TYPE run
                           VALUE SET VIEW PY FILE code/satcodes.py ON DATABASE DIRECTORY
                           """  ) 
 
        return self.db.Prompt( "WEB FORM GET CONTROLS" )

    # Entry point
    def main( self ):
        return self.get_form()
