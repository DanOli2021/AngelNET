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

        result = self.db.Prompt( "GET ACCOUNTS" )
        print( self.message )
        print( result )
        #End Your code here

        return result

    