select StockSymbol, V, Date, C
from StockSymbolHistory 
where Date > '2022-02-01' and Date < '2022-03-01' 
and V = 100 and C < 10000
order by StockSymbol desc,date desc


select StockSymbol, V, Date, C
from StockSymbolHistory 
where Date > '2022-02-01' and Date < '2022-03-01' 
and V > 0 
and V < 1000 and C < 10000
order by StockSymbol desc,date desc