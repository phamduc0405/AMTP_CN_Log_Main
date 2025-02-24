using ACO2_App._0;
using ACO2_App._0.INIT;
using ACO2_App._0.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using Binding = System.Windows.Data.Binding;

namespace ACO2.Views.Data
{
    /// <summary>
    /// Interaction logic for DataProductView.xaml
    /// </summary>
    public partial class DataProductView : UserControl
    {
        private Controller _controller;
        private List<CellData> _data;

        public DataProductView()
        {
            InitializeComponent();
            _controller = MainWindow.Controller;
            CreateEvent();
            CreateDynamicColumns();

         
            dtpEnd.SelectedDate = DateTime.Now;
            dtpStart.SelectedDate = DateTime.Now;
    

        }
        private void CreateEvent()
        {
            btnLoad.Click += (s, e) =>
            {
                CsvLoad(DefaultData.LogPath);
            };
            btnSearch.Click += (s, e) =>
            {
                //FilterData();
            };
            cbbEquipment.SelectionChanged += (s, e) =>
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    string zonenumber = cbbEquipment.SelectedItem.ToString().Replace("ZONE", "").Trim();
                    if (zonenumber != "ALL") { _data = _data.FindAll(x => x.ZoneNo.ToUpper().Contains(zonenumber)); }
                    else { FilterData(); }
                    PopulateListView(_data);
                }));
            };
        }



        private void CreateDynamicColumns()
        {
            cbbEquipment.Items.Clear();
            cbbEquipment.Items.Add($"ALL ZONE");
            foreach(var eqp in _controller.Equipment)
            {
                cbbEquipment.Items.Add($"ZONE {eqp.EqpConfig.EQPIndex+1}");
            }
            cbbEquipment.SelectedIndex = 0;

            grdView.Columns.Clear();

            // Danh sách Header và Binding Path tương ứng
            var columns = new (string Header, string BindingPath)[]
            {
            ("TIME", "Time"),
            ("CELLID", "CellID"),
            ("EQUIP", "MachineName"),
            ("ZONE_NO", "ZoneNo"),
            ("UNIT", "Unit"),
            ("STAGE", "Stage"),
            ("CHANNEL", "Channel.ChannelNo"),
            ("Trace In", "TrackIn"),
            ("Trace Out", "TrackOut"),

            ("INS_ROBOT 1_TOOL", "InsRobot1ToolNo"),
            ("INS_ROBOT 2_TOOL", "InsRobot2ToolNo"),

            ("CONTACT_RESULT", "Channel.ContactResult"),
            ("LAST RESULT", "Channel.LastResult"),
            ("DEFECTCODE", "Channel.DefectCode"),
            ("MTP_WRITE_RESULT", "Channel.MTPWriteResult"),

            ("AB Rule", "ABRule"),
            ("RETRY", "Retry"),
            ("RECHECKED", "Rechecked"),

            ("TEMPERATER", "Temperater"),

            ("MC_VER", "Channel.MCVer"),
            ("TX_HOST_VER", "Channel.TxHostVer"),
            ("TMD FILE", "Channel.TMDFile"),
            ("PG_UI", "Channel.PGUi"),
            ("T5 MAC CHANNEL", "Channel.T5MacChannel"),
            ("X600", "Channel.X600"),

            ("IBat", "Channel.Ibat"),
            ("IVss", "Channel.IVss"),
            ("IDd", "Channel.IDd"),
            ("ICi", "Channel.ICi"),
            ("IBat2", "Channel.IBat2"),
            ("IDd2", "Channel.IDd2"),

            ("MC_START_TIME", "MCStartTime"),
            ("MC_END_TIME", "MCEndTime"),
            ("MC_TACT TIME", "MCTackTime"),
            ("UNIT TOTAL TACT START TIME", "UnitStartTime"),
            ("UNIT TOTAL TACT END TIME", "UnitEndTime"),
            ("UNIT TOTAL TACT", "UnitTackTime"),

            ("INS_START_TIME", "Channel.InsStartTime"),
            ("INS_END_TIME", "Channel.InsEndTime"),
            ("INS_TACT TIME", "Channel.InsTackTime"),

            ("ROBOT PUT DOWN TACT START TIME", "RBDropStartTime"),
            ("ROBOT PUT DOWN TACT END TIME", "RBDropEndTime"),
            ("ROBOT PUT DOWN TACT", "RBDropTackTime"),

            ("CONTACT START_TIME", "Channel.ContactStartTime"),
            ("CONTACT END_TIME", "Channel.ContactEndTime"),
            ("CONTACT TACT TIME", "Channel.ContactTackTime"),

            ("SPARE TACT TIME", "SpareStartTime"),
            ("SPARE TACT END TIME", "SpareEndTime"),
            ("SPARE TACT", "SpareTackTime"),

            ("SERVO FW TACT START TIME", "ServoFWStartTime"),
            ("SERVO FW TACT END TIME", "ServoFWEndTime"),
            ("SERVO FW TACT", "ServoFWTaktTime"),

            ("CYL DW TACT START TIME", "CylDWStartTime"),
            ("CYL DW TACT END TIME", "CylDWEndTime"),
            ("CYL DW TACT", "CylDWTaktTime"),

            ("CHECK MTP_WRITE START TIME", "Channel.MTPStartTime"),
            ("CHECK MTP_WRITE TACT END TIME", "Channel.MTPEndTime"),
            ("CHECK MTP_WRITE TACT", "Channel.MTPTackTime"),

            ("CYL UP2 TACT START TIME", "CylUpStartTime"),
            ("CYL UP2 TACT END TIME", "CylUpEndTime"),
            ("CYL UP2 TACT", "CylUpTaktTime"),

            ("SERVO BW TACT START TIME", "ServoBWStartTime"),
            ("SERVO BW TACT END TIME", "ServoBWEndTime"),
            ("SERVO BW TACT", "ServoBWTaktTime"),

            ("ROBOT PICK UP TACT START TIME", "RBPickStartTime"),
            ("ROBOT PICK UP TACT END TIME", "RBPickEndTime"),
            ("ROBOT PICK UP TACT", "RBPickTackTime")
            };

            foreach (var column in columns)
            {
                // Các điều kiện để xác định width của các cột
                if (column.Header == "ZONE_NO" || column.Header == "Trace In" || column.Header == "Trace Out"
                    || column.Header == "INS_ROBOT 1_TOOL" || column.Header == "INS_ROBOT 2_TOOL"
                    || column.Header == "CHANNEL" || column.Header == "CONTACT_RESULT"
                    || column.Header == "MTP_WRITE_RESULT" || column.Header == "MC_TACT TIME"
                    || column.Header == "UNIT TOTAL TACT" || column.Header == "INS_TACT TIME"
                    || column.Header == "CONTACT TACT TIME" || column.Header == "MTPWRITE TACT"
                    || column.Header == "AB Rule" || column.Header == "RETRY" || column.Header == "RECHECKED" || column.Header == "UNIT" || column.Header == "STAGE" || column.Header == "EQUIP")
                {
                    // Những trường cần có width = 90 (sử dụng các trường kiểu double và những trường đặc biệt)
                    grdView.Columns.Add(new GridViewColumn
                    {
                        Header = column.Header,
                        DisplayMemberBinding = new Binding(column.BindingPath),
                        Width = 90
                    });
                }
                else
                {
                    // Các cột khác có width = 150
                    grdView.Columns.Add(new GridViewColumn
                    {
                        Header = column.Header,
                        DisplayMemberBinding = new Binding(column.BindingPath),
                        Width = 150
                    });
                }
            }

        }

        public void CsvLoad(string path = "")
        {
            try
            {
                DateTime startDate = dtpStart.SelectedDate ?? DateTime.MinValue;
                DateTime endDate = dtpEnd.SelectedDate ?? DateTime.MaxValue;

                if (startDate == null || endDate == null) return;

                int result = startDate.CompareTo(endDate);
                if (result > 0) return;
               
                 _data = _controller.LoadDataByDateRange(startDate, endDate);
                PopulateListView(_data);
            }
            catch (Exception ex)
            {
                var debug = string.Format("Class:{0} Method:{1} exception occurred. Message is <{2}>.", this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex.Message);
                LogTxt.Add(LogTxt.Type.Exception, debug);
            }

        }

        private void FilterData()
        {
            string filterKeyword = txtSearch.Text.ToUpper();
            if (string.IsNullOrWhiteSpace(filterKeyword))
            {
                CsvLoad();
                return;
            }
            else
            {
                _data = _data.FindAll(x => x.CellID.ToUpper().Contains(filterKeyword) 
                 || x.Channel.ChannelNo.ToString().ToUpper() == filterKeyword);

                string zoneNumber = cbbEquipment.SelectedItem.ToString().Replace("ZONE", "");
                if (zoneNumber != "ALL") { _data = _data.FindAll(x => x.ZoneNo.ToUpper().Contains(zoneNumber)); }

                PopulateListView(_data);
            }
        }

        private void PopulateListView(List<CellData> productDataList)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                listView.ItemsSource = productDataList;
            }));
            // Set ItemsSource

        }



    }
}
