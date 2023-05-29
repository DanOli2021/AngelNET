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

        self.db.Prompt( """WEB FORM CONTROL header1 
                           TYPE title 
                           VALUE WebMi GUI
                           CLASS row justify-content-center 
                           STYLE font-family:Arial; font-size:xx-large; font-weight:bolder"""  ) 

        self.db.Prompt( """WEB FORM CONTROL header2 
                           TYPE title
                           VALUE The greatest power in the universe is simplicity
                           CLASS row justify-content-center 
                           STYLE font-family:Arial; font-size:medium; font-weight:bolder 
                           ROW SPACE row mt-4"""  ) 

        self.db.Prompt( """WEB FORM CONTROL button1
                           TYPE button
                           VALUE Learn more
                           CLASS btn btn-primary
                           STYLE width:100%; height:120px; background-color: #1B7827; font-family:Arial; font-size:x-large; font-weight:bolder;
                           ROW SPACE row mt-2                           
                           COMMAND SET VIEW PY FILE webmi.py ON MAIN DIRECTORY""")
 
        return self.db.Prompt( "WEB FORM GET CONTROLS" )

    # Entry point
    def main( self ):
        return self.get_form()


