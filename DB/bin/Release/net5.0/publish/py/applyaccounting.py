import json
import locale

locale.setlocale( locale.LC_ALL, "" )

class accounting:

    #db -> Database access variable
    #data -> Json data passed by the application 
    def __init__(self, db, data):
            self.db = db
            self.data = data

    def create_tables(self):
            # We create the tables we need
            self.db.Prompt( "CREATE TABLE policies FIELD LIST period, operation_date, description, owner, owner_name, invoice, digestion, policie, total STORAGE main", True )
            self.db.Prompt( "CREATE TABLE invoicecounter FIELD LIST counter NUMERIC STORAGE main", True )


    def invoice_counter( self, invoice ):
            result = self.db.Prompt( "SELECT * FROM invoicecounter PARTITION KEY " + invoice.replace(" ", "_") + " WHERE id = '" + invoice + "'", True )

            if result == "{}":
                    invoicecounter = {}
                    invoicecounter["id"] = invoice
                    invoicecounter["counter"] = 1
                    self.db.Prompt( "INSERT INTO invoicecounter PARTITION KEY " + invoice.replace(" ", "_") + " VALUES " + json.dumps(invoicecounter,4), True )
                    return "1"

            datatable = json.loads( result )
            counter = int( datatable["invoicecounter"][0]["counter"] )
            counter += 1

            invoicecounter = {}
            invoicecounter["id"] = invoice
            invoicecounter["counter"] = counter
            self.db.Prompt( "INSERT INTO invoicecounter PARTITION KEY " + invoice.replace(" ", "_") + " VALUES " + json.dumps(invoicecounter,4), True )
            return str(counter)

    def apply_accounting(self, db, data):
            self.db = db
            self.data = data

            result = self.create_tables()
            policy = json.loads(self.data)

            debit = 0.0
            credit = 0.0

            if str(policy["id"]) == "None":
                    raise Exception("The unique identifier of the policy was not indicated")       

            if str(policy["owner"]) == "None":
                    raise Exception("The unique identifier of the origin of the database that generated the policy has not been indicated")       

            for value in policy["details"]:
                    debit += value["debit"]
                    credit += value["credit"]
                    
                    if str(value["id"]) == "None":
                            raise Exception("The unique identifier of each policy account has not been indicated " + value["account"])       

            if debit != credit:
                    raise Exception("Credit and debit are not the same, Debit {Debit:.2f} Credit {Credit:.2f}".format( Debit = debit, Credit = credit ))       

            result = self.invoice_counter(policy["invoice"])
            invoicestring = policy["invoice"] + " " + result

            result = self.db.Prompt( "SELECT * FROM periods PARTITION KEY periods WHERE id > '" + policy["period"] + "'", True )
            periods = json.loads(result)

            total = 0.0
            
            for value in policy["details"]:
                    self.db.Prompt( "BUSINESS ACCOUNTING PERIOD " + policy["period"] + " GUID " + value["id"] + " ACCOUNT " + value["account"] + " OPERATION DATE " + value["date_of_capture"] + " DESCRIPTION " + value["detail_description"] + " DOCUMENT " + invoicestring + " DEBIT " + str(value["debit"]) + " CREDIT " + str(value["credit"]), True )
                    self.db.Prompt( "BUSINESS CARRY BALANCES PERIOD " + policy["period"] + " ACCOUNT " +  value["account"] + " OPERATION DATE " + policy["operation_date"], True)
                    self.db.Prompt( "BUSINESS UPDATE BALANCE PERIOD " + policy["period"] + " ACCOUNT " +  value["account"], True)
                    self.db.Prompt( "BUSINESS UPDATE MASTER PERIOD " + policy["period"] + " ACCOUNT " +  value["account"], True)

                    total += value["debit"]

                    actualperiod = policy["period"]                    
                    
                    if result != "{}":
                        for period in periods["periods"]:
                                if str(period["closed"]) == "1":
                                        raise Exception("There is a higher period is closed and cannot be affected: Period " + period["period"])

                                self.db.Prompt( "BUSINESS TRANSFER BALANCES INITIAL PERIOD " + actualperiod + " DESTINATION PERIOD " +  period["id"] + " ACCOUNT " + value["account"], True)
                                actualperiod = period["id"]

            policyheader = {}
            policyheader["id"] = policy["id"]
            policyheader["period"] = policy["period"]
            policyheader["operation_date"] = policy["operation_date"]
            policyheader["description"] = policy["description"]
            policyheader["owner"] = policy["owner"]
            policyheader["owner_name"] = policy["owner_name"]
            policyheader["invoice"] = invoicestring
            policyheader["total"] = total
            policyheader["policie"] = self.data

            self.db.Prompt( "INSERT INTO policies PARTITION KEY " + policy["period"] + " VALUES " + json.dumps(policyheader,4), True )       
            return "Ok. Policie created: " + invoicestring + " GUID: " + policy["id"]
