import uuid
import matplotlib.pyplot as plt

#Thread issues
plt.switch_backend('agg')

# If you want to use parameters from db, you need to use the following code:
class mainclass:

    def __init__(self):
        self.db = None
        self.server_db = None
        self.message = None


    def main( self, db, server_db, message):
    
        #Your code here
        self.db = db
        self.server_db = server_db
        self.message = message

        # Datos
        x = [1, 2, 3, 4, 5]
        y = [1, 4, 9, 16, 25]
    
        plt.close('all')
        # Crear el plot
        plt.plot(x, y)
    
        # Titular y etiquetar ejes
        plt.title('Ejemplo Simple de Plot')
        plt.xlabel('Eje X')
        plt.ylabel('Eje Y')
    
        filename = str(uuid.uuid4()) + ".png"
        save_filename = "C:/Desarrollo/AngelNET/AngelSQLServer/wwwroot/images/" + filename

        # Salvar el plot
        plt.savefig(save_filename)

        print("Plot saved to " + filename)

        html = "<html><body><img src='/images/" + filename + "'></body></html>"
        #End Your code here

        return html

    
