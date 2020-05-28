IF EXISTS (SELECT 1 FROM dbo.sysobjects WHERE id = OBJECT_ID('sp_QueryProductData') AND OBJECTPROPERTY(id,'IsProcedure') = 1)    
  --   ɾ���洢����    
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

	SELECT ID AS ���,stationPressureSet AS ѹ������,stationShiftSet AS λ������,stationPressureApex AS ѹ����ֵ,stationShiftApex AS λ�Ʒ�ֵ,pressureResult AS ѹװ��� FROM dbo.Press WHERE SerialNumber = @sn
	
	SELECT c.parameterName AS ������,c.parameterValue AS ����ֵ FROM (SELECT a.SerialNumber,CAST(measureResult AS DECIMAL(10,4)) AS �����,b.checkResult AS ��Ƭ�����,b.calculateResult AS ��϶������,a.MasterMeasure AS �궨����
	FROM dbo.CLXDCLZ a LEFT JOIN dbo.CLXDZ b ON a.SerialNumber = b.SerialNumber) b UNPIVOT 
	( parameterValue FOR parameterName IN 
	(�����,��Ƭ�����,��϶������,�궨����)) AS c
	 WHERE c.SerialNumber = @sn
	
	SELECT a.parameterName AS ������,a.parameterValue AS ����ֵ FROM (
SELECT topChamberUpperPressLimit AS ��ǻѹ������,topChamberLowerPressLimit AS ��ǻѹ������,topChamberLeakValue AS ��ǻй©ֵ,CAST(topChamberDecisionResult AS DECIMAL(10,4)) AS ��ǻ�жϽ��,
middleChamberUpperPressLimit AS ��ǻѹ������,middleChamberLowerPressLimit AS ��ǻѹ������,middleChamberLeakValue AS ��ǻй©ֵ,CAST(middleChamberDecisionResult AS DECIMAL(10,4)) AS ��ǻ�жϽ��,
bottomChamberUpperPressLimit AS ��ǻѹ������,bottomChamberLowerPressLimit AS ��ǻѹ������,bottomChamberLeakValue AS ��ǻй©ֵ,CAST(bottomChamberDecisionResult AS DECIMAL(10,4)) AS ��ǻ�жϽ�� FROM dbo.LeakTest WHERE SerialNumber = @sn
) a UNPIVOT ( parameterValue FOR parameterName IN (a.��ǻѹ������,a.��ǻѹ������,a.��ǻй©ֵ,a.��ǻ�жϽ��,a.��ǻѹ������,a.��ǻѹ������,a.��ǻй©ֵ,a.��ǻ�жϽ��,a.��ǻѹ������,a.��ǻѹ������,a.��ǻй©ֵ,a.��ǻ�жϽ��)) AS a

	SELECT ID AS ���,StationCode AS ��λ,torqueLimitUpper AS ��������,torqueLimitLower AS ��������,angleLimitUpper AS �Ƕ�����,angleLimitLower AS �Ƕ�����,torque AS ʵ������,angle AS ʵ�ʽǶ�,tightenResult AS �ж� FROM dbo.Tighten WHERE SerialNumber = @sn

END


GO