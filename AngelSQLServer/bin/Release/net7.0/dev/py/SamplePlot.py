import matplotlib.pyplot as plt

def simple_plot():
    # Datos
    x = [1, 2, 3, 4, 5]
    y = [1, 4, 9, 16, 25]
    
    # Crear el plot
    plt.plot(x, y)
    
    # Titular y etiquetar ejes
    plt.title('Ejemplo Simple de Plot')
    plt.xlabel('Eje X')
    plt.ylabel('Eje Y')
    
    # Mostrar el plot
    plt.show()

simple_plot()
