import pandas as pd
import json
from fbprophet import Prophet

# Cargar los datos desde el archivo JSON
with open('c:/daniel/results.json', 'r') as file:
    data = json.load(file)

# Convertir los datos a un DataFrame
df = pd.DataFrame(data)

# Convertir la columna 'fecha' a tipo datetime
df['fecha'] = pd.to_datetime(df['fecha'])

# Agrupar los datos por fecha y código de producto para obtener la cantidad total vendida por día
df_grouped = df.groupby(['fecha', 'articulo'])['cantidad'].sum().reset_index()

# Preparar los datos para Prophet
df_grouped.rename(columns={'fecha': 'ds', 'cantidad': 'y'}, inplace=True)

# Entrenar un modelo para cada código de producto y predecir la cantidad de ventas del siguiente día
predictions = {}
unique_articles = df_grouped['articulo'].unique()

for article in unique_articles:
    df_article = df_grouped[df_grouped['articulo'] == article]
    
    model = Prophet(daily_seasonality=True)
    model.fit(df_article)
    
    future = model.make_future_dataframe(periods=1, freq='D')
    forecast = model.predict(future)
    
    predictions[article] = forecast['yhat'].values[-1]

# El diccionario 'predictions' contiene las predicciones para cada código de producto
print(predictions)
