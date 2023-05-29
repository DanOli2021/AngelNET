import json
import locale

locale.setlocale( locale.LC_ALL, "" )

class mainclass:

    #db -> Database access variable
    #data -> Json data passed by the application 
    def __init__(self, db, data):
            self.db = db
            self.data = data
            self.param = json.loads(self.data)

    def copy_table(self):
        analizer = self.db.SyntaxAnalizer() 
        select_parts = analizer.Interpreter(self.param["from"])

        if analizer.errorString != "":
            raise Exception(analizer.errorString)

        result = self.db.Prompt("GET TABLES WHERE tablename = '" + select_parts["from"] + "'", True)

        if result == "[]":
            raise Exception("Error: Source table does not exits:" + select_parts["from"] )

        source_table = json.loads(result)

        if self.param["copy_to"] == select_parts["from"]:
            raise Exception( "Error: The source table and the destination table cannot be the same" )

        sql = "CREATE TABLE " + self.param["copy_to"] + " FIELD LIST " + source_table[0]["fieldlist"] + "STORAGE " + self.param["storage"]   

        self.db.Prompt( sql, True )  

        rows_limit = 2000
        off_set = 0


        while True:

            sql = ""

            if off_set == 0:
                sql = self.param["from"] + " LIMIT " + str( rows_limit )
            else: 
                sql = self.param["from"] + " LIMIT " + str( rows_limit ) + " OFFSET " + str( off_set )

            off_set += rows_limit

            result = self.db.Prompt( sql, True )

            if result == "{}":
                break

            partitions = json.loads( result )
 
            for row in partitions:
                self.db.Prompt( "INSERT INTO " + self.param["copy_to"] + " PARTITION KEY " + row + " VALUES " + json.dumps(partitions[row],4), True )

            print( "Rows procesed: " + str( off_set ) )

        return "Ok."


    def main(self):
        return self.copy_table()



