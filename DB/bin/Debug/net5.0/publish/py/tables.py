from ast import Param
import json
import csv

class tables:

    #db -> Database access variable
    #data -> Json data passed by the application 
    def __init__(self, db, data):
            self.db = db
            self.data = data

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



    def import_from_csv(self):

        data = {}
        
        csv_file = open(self.param["import_from"], 'r')
        csv_dicreader = csv.DictReader(csv_file)

        csv_dicreader.fieldnames

        fields_to_create_table = csv_dicreader.fieldnames[:]

        if "id" in fields_to_create_table:
            fields_to_create_table.remove("id")

        fieldlist = ",".join(fields_to_create_table)

        if self.param["type_search"] == "false":  
           self.db.Prompt("CREATE TABLE " + self.param["to"] + " FIELD LIST " + fieldlist + " STORAGE " + self.param["storage"], True )
        else:
           self.db.Prompt("CREATE TABLE " + self.param["to"] + " FIELD LIST " + fieldlist + " STORAGE " + self.param["storage"] + " TYPE SEARCH", True )

        rows_count = 0
        rows_message = 0

        my_list = []

        for row in csv_dicreader:
             rows_count += 1
             rows_message += 1

             my_list.append( row )

             if rows_message == 2000:
                self.db.Prompt("INSERT INTO " + self.param["to"] + " PARTITION KEY " + self.param["to"] + " VALUES " + json.dumps(my_list,4), True )
                my_list = []
                print( "Records processed: " + str( rows_count ) )
                rows_message = 0

        if len( my_list ) > 0:
            self.db.Prompt("INSERT INTO " + self.param["to"] + " PARTITION KEY " + self.param["to"] + " VALUES " + json.dumps(my_list,4), True )

        print( "Records finally processed: " + str( rows_count ) )
        return "Ok."
