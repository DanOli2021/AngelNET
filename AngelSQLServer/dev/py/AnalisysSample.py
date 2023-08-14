import json
import pandas as pd
from statsmodels.tsa.arima.model import ARIMA
from statsmodels.tsa.stattools import adfuller

# Carga de datos
with open('c:/daniel/results.json', 'r') as file:
    data = json.load(file)

# Convertir los datos a un DataFrame
df = pd.DataFrame(data)

# Convertir la columna 'fecha' a tipo datetime
df['fecha'] = pd.to_datetime(df['fecha'])

# Agrupar los datos por fecha y código de producto para obtener la cantidad total vendida por día
df_grouped = df.groupby(['fecha', 'articulo'])['cantidad'].sum().reset_index()

# Función para determinar el valor de d para hacer la serie estacionaria
def get_d_value(series):
    result = adfuller(series)
    if result[1] <= 0.05:  # p-value
        return 0
    else:
        result = adfuller(series.diff().dropna())
        if result[1] <= 0.05:
            return 1
    return 2

# Prediciendo ventas usando ARIMA para cada producto
arima_predictions = {}
unique_articles = df_grouped['articulo'].unique()

for article in unique_articles:
    df_article = df_grouped[df_grouped['articulo'] == article].copy()

    print( df_article )
    
    # Determinar el valor de d
    d = get_d_value(df_article['y'])
    
    # Usamos un simple modelo ARIMA(1,d,1) aquí, pero en la práctica, podrías querer encontrar los mejores valores p, d, q
    try:
        model = ARIMA(df_article['y'], order=(1, d, 1))
        model_fit = model.fit()
        
        # Pronosticar el siguiente valor
        forecast = model_fit.forecast(steps=1)[0]
        arima_predictions[article] = forecast[0]
    except:
        arima_predictions[article] = None
