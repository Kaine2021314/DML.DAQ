IF EXISTS (SELECT 1 FROM dbo.sysobjects WHERE id = OBJECT_ID('sp_QueryProductData') AND OBJECTPROPERTY(id,'IsProcedure') = 1)    
  --   删除存储过程    
DROP PROCEDURE sp_QueryProductData
GO  

CREATE PROCEDURE sp_QueryProductData
@beginTime DATETIME = NULL,
@endTime DATETIME = NULL,
@sn VARCHAR(100) = NULL

AS

IF @beginTime IS NOT NULL AND @endTime IS NOT NULL AND @sn IS NULL
BEGIN

	SELECT OnLineTime,ProductionOrderCode,g.SerialNumber,MaterielCode,ProductType,PalletCode,
dbo.CLXDCLZ.MasterMeasure,dbo.CLXDCLZ.measureLimitUpper,dbo.CLXDCLZ.measureLimitLower, 
dbo.CLXDCLZ.measureValue,c.checkResult,c.calculateResult,c.gasketNumber,c.gapValue,
l.topChamberUpperPressLimit,l.middleChamberUpperPressLimit,l.bottomChamberUpperPressLimit,
l.pressureRetentionTime,l.topChamberLeakValue,l.middleChamberLeakValue,l.bottomChamberLeakValue,
p.stationPressureSet,p.stationShiftSet,p.stationPressureApex,p.stationShiftApex,
t.torqueLimitUpper,t.torqueLimitLower,t.angleLimitUpper,t.angleLimitLower,t.angle,t.torque
FROM dbo.GoodsOrder AS g LEFT JOIN
dbo.CLXDCLZ ON g.SerialNumber = dbo.CLXDCLZ.SerialNumber
LEFT JOIN dbo.CLXDZ AS c ON g.SerialNumber = c.SerialNumber
LEFT JOIN dbo.LeakTest AS l ON g.SerialNumber = l.SerialNumber
LEFT JOIN dbo.Press AS p ON g.SerialNumber = p.SerialNumber
LEFT JOIN dbo.Tighten AS t ON g.SerialNumber = t.SerialNumber
LEFT JOIN dbo.Oiling AS o ON g.SerialNumber = o.SerialNumber
WHERE g.OnLineTime BETWEEN @beginTime AND @endTime

END

ELSE IF @sn IS NOT NULL
BEGIN
	SELECT * FROM dbo.GoodsOrder

	SELECT ID AS 序号,stationPressureSet AS 压力设置,stationShiftSet AS 位移设置,stationPressureApex AS 压力峰值,stationShiftApex AS 位移峰值,pressureResult AS 压装结果 FROM dbo.Press WHERE SerialNumber = @sn
	
	SELECT c.parameterName AS 参数名,c.parameterValue AS 参数值 FROM (SELECT a.SerialNumber,CAST(measureResult AS DECIMAL(10,4)) AS 检测结果,b.checkResult AS 垫片检测结果,b.calculateResult AS 间隙计算结果,a.MasterMeasure AS 标定块厚度
	FROM dbo.CLXDCLZ a LEFT JOIN dbo.CLXDZ b ON a.SerialNumber = b.SerialNumber) b UNPIVOT 
	( parameterValue FOR parameterName IN 
	(检测结果,垫片检测结果,间隙计算结果,标定块厚度)) AS c
	 WHERE c.SerialNumber = @sn
	
	SELECT a.parameterName AS 参数名,a.parameterValue AS 参数值 FROM (
SELECT topChamberUpperPressLimit AS 上腔压力上限,topChamberLowerPressLimit AS 上腔压力下限,topChamberLeakValue AS 上腔泄漏值,CAST(topChamberDecisionResult AS DECIMAL(10,4)) AS 上腔判断结果,
middleChamberUpperPressLimit AS 中腔压力上限,middleChamberLowerPressLimit AS 中腔压力下限,middleChamberLeakValue AS 中腔泄漏值,CAST(middleChamberDecisionResult AS DECIMAL(10,4)) AS 中腔判断结果,
bottomChamberUpperPressLimit AS 下腔压力上限,bottomChamberLowerPressLimit AS 下腔压力下限,bottomChamberLeakValue AS 下腔泄漏值,CAST(bottomChamberDecisionResult AS DECIMAL(10,4)) AS 下腔判断结果 FROM dbo.LeakTest WHERE SerialNumber = @sn
) a UNPIVOT ( parameterValue FOR parameterName IN (a.上腔压力上限,a.上腔压力下限,a.上腔泄漏值,a.上腔判断结果,a.中腔压力上限,a.中腔压力下限,a.中腔泄漏值,a.中腔判断结果,a.下腔压力上限,a.下腔压力下限,a.下腔泄漏值,a.下腔判断结果)) AS a

	SELECT ID AS 序号,StationCode AS 工位,torqueLimitUpper AS 力矩上限,torqueLimitLower AS 力矩下限,angleLimitUpper AS 角度上限,angleLimitLower AS 角度下限,torque AS 实际力矩,angle AS 实际角度,tightenResult AS 判定 FROM dbo.Tighten WHERE SerialNumber = @sn

END


GO