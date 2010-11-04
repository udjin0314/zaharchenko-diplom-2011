using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using DiplomWPF.Common;
using WPFChart3D;
using System;

namespace DiplomWPF.Client.UI
{
    class Graph3D
    {
        private Chart3D m_3dChart;
        private int m_nChartModelIndex = -1;

        private TransformMatrix m_transformMatrix = new TransformMatrix();

        private ViewportRect m_selectRect = new ViewportRect();
        private int m_nRectModelIndex = -1;

        private Viewport3D mainViewport;

        private Process process;

        public Graph3D(Viewport3D viewport)
        {
            mainViewport = viewport;
            initialize();
        }

        public void reDrawNewProcess(Process processIn)
        {
            initWithProcess(processIn);
            reDrawNewValues(0,0);
        }

        private void initWithProcess(Process processIn)
        {
            process = processIn;
            int nXNo = process.I;

            m_3dChart = new UniformSurfaceChart3D();
            ((UniformSurfaceChart3D)m_3dChart).SetGrid(nXNo, nXNo, -100, 100, -100, 100);
            TransformChart();
        }

        public void reDrawNewValues(int time, int zn)
        {
            prepareData(time, zn);
            TransformChart();
        }

        private void initialize()
        {
            m_selectRect.SetRect(new Point(-0.5, -0.5), new Point(-0.5, -0.5));
            WPFChart3D.Model3D model3d = new WPFChart3D.Model3D();
            ArrayList meshs = m_selectRect.GetMeshes();
            m_nRectModelIndex = model3d.UpdateModel(meshs, null, m_nRectModelIndex, mainViewport);
        }

        public void OnMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs args)
        {
            Point pt = args.GetPosition(mainViewport);
            if (args.ChangedButton == MouseButton.Left)         // rotate or drag 3d model
            {
                m_transformMatrix.OnLBtnDown(pt);
            }
            else if (args.ChangedButton == MouseButton.Right)   // select rect
            {
                m_selectRect.OnMouseDown(pt, mainViewport, m_nRectModelIndex);
            }
        }

        public void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs args)
        {
            Point pt = args.GetPosition(mainViewport);

            if (args.LeftButton == MouseButtonState.Pressed)                // rotate or drag 3d model
            {
                m_transformMatrix.OnMouseMove(pt, mainViewport);

                TransformChart();
            }
            else if (args.RightButton == MouseButtonState.Pressed)          // select rect
            {
                m_selectRect.OnMouseMove(pt, mainViewport, m_nRectModelIndex);
            }

        }

        public void OnMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs args)
        {
            Point pt = args.GetPosition(mainViewport);
            if (args.ChangedButton == MouseButton.Left)
            {
                m_transformMatrix.OnLBtnUp();
            }
            else if (args.ChangedButton == MouseButton.Right)
            {
                if (m_nChartModelIndex == -1) return;
                // 1. get the mesh structure related to the selection rect
                MeshGeometry3D meshGeometry = WPFChart3D.Model3D.GetGeometry(mainViewport, m_nChartModelIndex);
                if (meshGeometry == null) return;

                // 2. set selection in 3d chart
                m_3dChart.Select(m_selectRect, m_transformMatrix, mainViewport);

                // 3. update selection display
                m_3dChart.HighlightSelection(meshGeometry, Color.FromRgb(200, 200, 200));
            }
        }

        public void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs args)
        {
            m_transformMatrix.OnKeyDown(args);
            TransformChart();
        }

        private void UpdateModelSizeInfo(ArrayList meshs)
        {
            int nMeshNo = meshs.Count;
            int nChartVertNo = 0;
            int nChartTriangelNo = 0;
            for (int i = 0; i < nMeshNo; i++)
            {
                nChartVertNo += ((Mesh3D)meshs[i]).GetVertexNo();
                nChartTriangelNo += ((Mesh3D)meshs[i]).GetTriangleNo();
            }
        }

        private void TransformChart()
        {
            if (m_nChartModelIndex == -1) return;
            ModelVisual3D visual3d = (ModelVisual3D)(this.mainViewport.Children[m_nChartModelIndex]);
            if (visual3d.Content == null) return;
            Transform3DGroup group1 = visual3d.Content.Transform as Transform3DGroup;
            group1.Children.Clear();
            group1.Children.Add(new MatrixTransform3D(m_transformMatrix.m_totalMatrix));
        }

        private void prepareData(int time, int zn)
        {
            int nXNo = process.I;
            int nYNo = process.I;

            for (int i = 0; i < nXNo; i++)
                for (int j = 0; j < nXNo; j++)
                {
                    Vertex3D vert = m_3dChart[i * nYNo + j];
                    if (j == nXNo - 1) j = nXNo;
                    vert.x = (float)(i * process.hr * Math.Cos(2 * j * Math.PI / nXNo));
                    vert.y = (float)(i * process.hr * Math.Sin(2 * j * Math.PI / nXNo));
                    vert.z = (float)(process.getPoint(i, zn, time));

                }
            m_3dChart.GetDataRange();

            double zMin = m_3dChart.ZMin();
            double zMax = m_3dChart.ZMax();
            int nVertNo = m_3dChart.GetDataNo();
            for (int i = 0; i < nVertNo; i++)
            {
                Vertex3D vert = m_3dChart[i];
                double h = (vert.z - process.minTemperature) / (process.maxTemperature - process.minTemperature);

                Color color = WPFChart3D.TextureMapping.PseudoColor(h);
                m_3dChart[i].color = color;
            }

            ArrayList meshs = ((UniformSurfaceChart3D)m_3dChart).GetMeshes();

            UpdateModelSizeInfo(meshs);

            WPFChart3D.Model3D model3d = new WPFChart3D.Model3D();
            Material backMaterial = new SpecularMaterial(new SolidColorBrush(Colors.Gray),5);
            m_nChartModelIndex = model3d.UpdateModel(meshs, backMaterial, m_nChartModelIndex, this.mainViewport);

            float xMin = m_3dChart.XMin();
            float xMax = m_3dChart.XMax();
            m_transformMatrix.CalculateProjectionMatrix(xMin, xMax, xMin, xMax, zMin, zMax, 0.5);
        }


    }
}
