import json
import csv
import locale

locale.setlocale( locale.LC_ALL, "" )

class mainclass:

    #db -> Database access variable
    #data -> Json data passed by the application 
    def __init__(self, db, data):
            self.db = db
            self.data = data
            self.param = json.loads(self.data)

    def import_from_csv(self):

        data = {}
        
        csv_file = open(self.param["import_from"], encoding = "utf8", mode = "r")
        csv_dicreader = csv.DictReader(csv_file)

        fields_to_create_table = csv_dicreader.fieldnames[:]

        if "id" in fields_to_create_table:
            fields_to_create_table.remove("id")

        if "PartitionKey" in fields_to_create_table:
            fields_to_create_table.remove("PartitionKey")

        fieldlist = ",".join(fields_to_create_table)

        sql_create_table = ""  

        if self.param["type_search"] == "false":  
           sql_create_table = "CREATE TABLE " + self.param["to"] + " FIELD LIST " + fieldlist + " STORAGE " + self.param["storage"]
        else:
           sql_create_table =  "CREATE TABLE " + self.param["to"] + " FIELD LIST " + fieldlist + " STORAGE " + self.param["storage"] + " TYPE SEARCH"

        self.db.Prompt( sql_create_table, True )

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


    def main( self ):

        if self.param["file_type"] == "csv":
           return self.import_from_csv()

        return "Ok."



