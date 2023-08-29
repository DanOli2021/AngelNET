import pandas as pd
import matplotlib.pyplot as plt
from statsmodels.tsa.arima.model import ARIMA

# Simulando datos de ventas diarias durante 30 días
data = {'Fecha': pd.date_range(start='2023-01-01', end='2023-01-30', freq='D'),
        'Ventas': [200, 220, 250, 275, 300, 320, 350, 370, 400, 420, 450, 475, 500, 520, 550, 570, 600, 620, 650, 675, 700, 720, 750, 770, 800, 820, 850, 875, 900, 920]}
df = pd.DataFrame(data)
df.set_index('Fecha', inplace=True)

# Visualizar los datos
plt.figure(figsize=(12,6))
plt.plot(df['Ventas'])
plt.title('Ventas Diarias')
plt.xlabel('Fecha')
plt.ylabel('Ventas')
plt.show()

# Crear el modelo ARIMA
model = ARIMA(df['Ventas'], order=(1,1,1))  # puedes ajustar los parámetros (p,d,q)
model_fit = model.fit()

# Pronóstico para los próximos 7 días
forecast = model_fit.forecast(steps=7)
print(f"Pronóstico de ventas para los próximos 7 días: {forecast}")

# Visualizar el pronóstico
plt.figure(figsize=(12,6))
plt.plot(df['Ventas'], label='Ventas pasadas')
plt.plot(pd.date_range(start='2023-01-31', periods=7, freq='D'), forecast, label='Pronóstico')
plt.title('Pronóstico de Ventas')
plt.xlabel('Fecha')
plt.ylabel('Ventas')
plt.legend()
plt.show()
