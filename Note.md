# Run migrate database:
- Add-Migration InitialCreate
- Update-Database


# Run to get date value between c# & Php
https://www.w3schools.com/php/phptryit.asp?filename=tryphp_compiler
echo date("Y-m-d H:i:s", 1388516401);
echo strtotime("2022-02-22 00:00:01");
echo "-";
echo strtotime("2022-02-24 00:00:01");

# Common sql
select Date, count(Date) as CDate, StockSymbol
from StockSymbolTradingHistory
where StockSymbol = 'DIG'
group by Date, StockSymbol
having count(Date) > 0

select * from StockSymbolTradingHistory
where StockSymbol = 'DIG' and Date = '2022-03-01T09:15:51'
order by date asc

--select count(*) from StockSymbolTradingHistory where IsTangDotBien = 'true'

select distinct StockSymbol 
from StockSymbolTradingHistory 
where IsTangDotBien = 'true'
and Date > '2022-03-03T00:00:00'

select *
from StockSymbolTradingHistory 
where StockSymbol = 'AAA'
and Date > '2022-03-03T00:00:00'
order by date         
         