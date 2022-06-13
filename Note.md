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
select top 10 * from StockSymbolHistory where StockSymbol = 'VCB' order by Date desc;

select top 10 * from KLGDMuaBan where StockSymbol = 'VCB' order by Date desc;

select top 10 * from StockSymbolFinanceYearlyHistory where StockSymbol = 'BWE' ;

select * from StockSymbolFinanceHistory where StockSymbol = 'BWE' 
--and Type = 1
--NameEn = 'Profit after tax for shareholders of the parent company' 
and (NameEn = 'Profit after tax for shareholders of parent company' or NameEn = 'P/E')
and YearPeriod = 2021
order by YearPeriod desc, Quarter desc;

select * from StockSymbolFinanceYearlyHistory
where 
StockSymbol in (select distinct StockSymbol from StockSymbolFinanceYearlyHistory where 
(NameEn = 'Profit after tax for shareholders of parent company')
and YearPeriod = 2021)
and NameEn = 'Net profit';

select * from StockSymbolFinanceYearlyHistory
where 
StockSymbol = 'SSH'
and NameEn = 'Net profit';

select * from StockSymbolFinanceYearlyHistory
where 
NameEn = 'Net profit' and Value is null
order by StockSymbol, YearPeriod;	

select * from StockSymbolFinanceYearlyHistory
where 
StockSymbol = 'AAA' 
and (NameEn = 'Net profit'
or NameEn = 'XV. Net profit atttributable to the equity holders of the Bank (XIII-XIV)'
or NameEn = 'Profit after tax for shareholders of parent company')
order by StockSymbol, YearPeriod desc;	


select * from StockSymbolFinanceHistory
where 
StockSymbol = 'ACB' 
and (NameEn = 'Net profit'
or NameEn = 'XV. Net profit atttributable to the equity holders of the Bank (XIII-XIV)'
or NameEn = 'Profit after tax for shareholders of parent company'
or NameEn = 'XV. Net profit atttributable to the equity holders of the Bank ')
order by StockSymbol, YearPeriod desc;	

select * from StockSymbolFinanceHistory
where 
StockSymbol = 'HAC' 
and (NameEn = 'Net profit'
or NameEn = 'XV. Net profit atttributable to the equity holders of the Bank (XIII-XIV)'
or NameEn = 'Profit after tax for shareholders of parent company'
or NameEn = 'XV. Net profit atttributable to the equity holders of the Bank '
or NameEn = '11.1. Profit after tax for shareholders of the parents company')
order by StockSymbol, YearPeriod desc;	

select * from StockSymbolFinanceHistory
where 
StockSymbol = 'CEO' 
and (NameEn = 'Net profit'
or NameEn = 'XV. Net profit atttributable to the equity holders of the Bank (XIII-XIV)'
or NameEn = 'Profit after tax for shareholders of parent company'
or NameEn = 'XV. Net profit atttributable to the equity holders of the Bank '
or NameEn = '11.1. Profit after tax for shareholders of the parents company')
order by StockSymbol, YearPeriod desc;	


select * from StockSymbolFinanceHistory
where 
StockSymbol = 'ACB' 
and Type = 1
and YearPeriod = 2021
order by StockSymbol, YearPeriod desc;	


select * from StockSymbolFinanceYearlyHistory
where 
StockSymbol = 'AAA' 
and (NameEn = 'Net profit'
or NameEn = 'XV. Net profit atttributable to the equity holders of the Bank (XIII-XIV)'
or NameEn = 'Profit after tax for shareholders of parent company'
or NameEn = 'XV. Net profit atttributable to the equity holders of the Bank '
or NameEn = '11.1. Profit after tax for shareholders of the parents company')
order by StockSymbol, YearPeriod desc;	

select * from KLGDMuaBan
where 
StockSymbol = 'ACB' order by Date desc;
