using DiGi.Geometry.Planar.Interfaces;
using DiGi.Geometry.Visual.Core.Interfaces;
using DiGi.Geometry.Visual.Planar.Interfaces;
using System;

namespace DiGi.BDOT10k.UI.Interfaces
{
    public interface IObiektGeometryczny: IVisual2D
    {
        Type GetUnderlyingType();
    }

    public interface IObiektGeometryczny<T, X> : IObiektGeometryczny, IVisual2D<T, X> where T : IGeometry2D where X : IAppearance
    {

    }
}
