using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Windows.Forms;
using AplicacionFalp;
using Falp;

namespace GES002i
{
    public partial class frmCasoGES : Form
    {
        ConectarFalp CnnFalp;
        Configuration Config;
        string[] Conexion = { "", "", "" };
        string Db_Usuario;
        DataTable Tbl_Caso = new DataTable();
        string v_observacion = "";
        string PCK = "PCK_GES002I";
        string val_caso = "";
        Int64 v_ID;
        
        public frmCasoGES()
        {
            InitializeComponent();
        }

        private void frmCasoGES_Load(object sender, EventArgs e)
        {
            if (!(CnnFalp != null))
            {
                ExeConfigurationFileMap FileMap = new ExeConfigurationFileMap();
                FileMap.ExeConfigFilename = Application.StartupPath + @"\..\WF.config";
                Config = ConfigurationManager.OpenMappedExeConfiguration(FileMap, ConfigurationUserLevel.None);

                CnnFalp = new ConectarFalp(Config.AppSettings.Settings["dbServer"].Value,//ConfigurationManager.AppSettings["dbServer"],
                                           Config.AppSettings.Settings["dbUser"].Value,//ConfigurationManager.AppSettings["dbUser"],
                                           Config.AppSettings.Settings["dbPass"].Value,//ConfigurationManager.AppSettings["dbPass"],
                                           ConectarFalp.TipoBase.Oracle);

                if (CnnFalp.Estado == ConnectionState.Closed) CnnFalp.Abrir(); // abre la conexion
                Conexion[0] = Config.AppSettings.Settings["dbServer"].Value;
                Conexion[1] = Config.AppSettings.Settings["dbUser"].Value;
                Conexion[2] = Config.AppSettings.Settings["dbPass"].Value;
               
                this.Text = this.Text + " [Versión: " + Application.ProductVersion + "] [Conectado: " + Conexion[0] + "]";

            }

            Db_Usuario = "SICI";
            Crea_Tabla();
            AsIgnaTag();
            txtDerivador.Text = "Fonasa";
            txtDerivador.Tag = 61603000;
            txtRespaldo.Focus();

        }


        private void AsIgnaTag()
        {
            txtFicha.Tag = string.Empty;
            txtDerivador.Tag = string.Empty;
            txtPatologia.Tag = string.Empty;
            txtEtapa.Tag = string.Empty;
            txtSubEtapa.Tag = string.Empty;
            txtPaquete.Tag = string.Empty;
            txtTipoPrev.Tag = string.Empty;
            txtPrevision.Tag= string.Empty;
            txtPlanPrev.Tag = string.Empty;
            txtRespaldo.Tag= string.Empty;
            grpDatosGES.Enabled = false;
            group_datos.Enabled = false;
            btnGrabarGes.Enabled = false;
            
        }


        private void Crea_Tabla()
        {
            Tbl_Caso.Columns.Add("ID_FILA", typeof(Int64));
            Tbl_Caso.Columns.Add("ID_PATOLOGIA", typeof(Int64));
            Tbl_Caso.Columns.Add("DESC_PATOLOGIA", typeof(string));
            Tbl_Caso.Columns.Add("ID_ETAPA", typeof(Int64));
            Tbl_Caso.Columns.Add("DESC_ETAPA", typeof(string));
            Tbl_Caso.Columns.Add("ID_SUB_ETAPA", typeof(Int64));
            Tbl_Caso.Columns.Add("DESC_SUB_ETAPA", typeof(string));
            Tbl_Caso.Columns.Add("ID_PAQUETE", typeof(Int64));
            Tbl_Caso.Columns.Add("DESC_PAQUETE", typeof(string));
            Tbl_Caso.Columns.Add("DIAS", typeof(Int64));
            Tbl_Caso.Columns.Add("FECHA_INICIO", typeof(string));
            Tbl_Caso.Columns.Add("FECHA_RECEPCION", typeof(string));
            Tbl_Caso.Columns.Add("OBSERVACION", typeof(string));
        }


        private void btnCargaPac_Click(object sender, EventArgs e)
        {
            Paciente.Select(0, 1, Conexion[0].ToString(), Conexion[1].ToString(), Conexion[2].ToString(), 1, true, Db_Usuario, "S");

            if (Paciente.Correlativo != 0)
            {
                if (Validar_estado_caso(Paciente.Correlativo))
                {
                    txtFicha.Tag = Paciente.Correlativo;
                    txtFicha.Text = Paciente.Ficha.ToString();
                    txtTipoDoc.Text = Paciente.Desc_TipoDoc;
                    if (Paciente.Desc_TipoDoc == "Rut")
                        txtDocumento.Text = Paciente.Documento + "-" + Paciente.DV;
                    else
                        txtDocumento.Text = Paciente.Documento;
                    txtNombre.Text = Paciente.Nombres;
                    txtPaterno.Text = Paciente.ApPaterno;
                    txtMaterno.Text = Paciente.ApMaterno;
                    string v_sexo = Paciente.Sexo;
                    if (v_sexo == "Ma")
                    {
                        txtSexo.Text = "M";
                    }
                    else
                    {
                        txtSexo.Text = "F";
                    }
                    txtFechaNac.Text = Paciente.Fecha_Nacimiento;
                    txtEdad.Text = Paciente.Edad.ToString();
                    txtECivil.Text = Paciente.Desc_Est_Civil;

                    txtTipoPrev.Text = Paciente.Desc_Tipo_Prev;
                    txtTipoPrev.Tag = Paciente.Cod_Tipo_Prev;

                    txtPrevision.Text = Paciente.Desc_Prevision;
                    txtPrevision.Tag = Paciente.Cod_Prevision;

                    txtPlanPrev.Text = Paciente.Desc_Convenio;
                    txtPlanPrev.Tag = Paciente.Cod_Plan_Prev;

                    grpDatosGES.Enabled = true;
                    Bloqueo_Datos_Generales();
                    btnDerivador.Focus();
                    btnAgregar.Enabled = true;
                }
                else
                {
                    MessageBox.Show("Estimado Usuario, El Paciente se encuentra con un Caso Abierto, por ende  No se puede Asociar otro Caso ", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);        
                                
                }
            }
        }

        private void TraeDerivador(ref AyudaSpreadNet.AyudaSprNet Ayuda, string descripcion)
        {
            string[] NomCol = { "ID", "RUT", "Derivador" };
            int[] AnchoCol = { 0, 60, 360 };
            Ayuda.Nombre_BD_Datos = CnnFalp.DBNombre;
            Ayuda.Pass = CnnFalp.DBPass;
            Ayuda.User = CnnFalp.DBUser;
            Ayuda.TipoBase = 1;
            Ayuda.NombreColumnas = NomCol;
            Ayuda.AnchoColumnas = AnchoCol;
            Ayuda.TituloConsulta = "Seleccionar Derivador";
            Ayuda.Package = PCK;
            Ayuda.Procedimiento = "P_TRAE_DERIVADOR";
            Ayuda.Generar_ParametroBD("PIN_DESCRIPCION", txtDerivador.Text.ToUpper(), DbType.String, ParameterDirection.Input);
            Ayuda.EjecutarSql();
        }
        
        private void btnDerivador_Click(object sender, EventArgs e)
        {
            txtDerivador.Text = string.Empty;
            txtDerivador.Tag = string.Empty;
            Cargar_Derivador();
        }

        private void btnPatologia_Click(object sender, EventArgs e)
        {
            txtPatologia.Text = string.Empty;
            txtPatologia.Tag = string.Empty;
            Cargar_Patalogia();
            txtPatologia.Enabled = false;
            btnPatologia.Enabled = false;
           
        }

        private void TraeDBParam(ref AyudaSpreadNet.AyudaSprNet Ayuda, Int32 TipoParam, string descripcion)
        {
            string[] NomCol = { "Código", "Descripción" };
            int[] AnchoCol = { 80, 350 };
            Ayuda.Nombre_BD_Datos = CnnFalp.DBNombre;
            Ayuda.Pass = CnnFalp.DBPass;
            Ayuda.User = CnnFalp.DBUser;
            Ayuda.TipoBase = 1;
            Ayuda.NombreColumnas = NomCol;
            Ayuda.AnchoColumnas = AnchoCol;
            Ayuda.TituloConsulta = "Seleccionar Patología";
            Ayuda.Package = PCK;
            Ayuda.Procedimiento = "P_CARGA_PATOLOGIA";
            Ayuda.Generar_ParametroBD("PIN_DESCRIPCION", descripcion.ToUpper(), DbType.String, ParameterDirection.Input);
            Ayuda.EjecutarSql();
        }

        private void btnEtapa_Click(object sender, EventArgs e)
        {
            txtEtapa.Text = string.Empty;
            txtEtapa.Tag = string.Empty;
            Cargar_Etapa();
            Cargar_Sub_Etapa();
        }

        private void TraeDBEtapa(ref AyudaSpreadNet.AyudaSprNet Ayuda,  string descripcion)
        {
            string[] NomCol = { "Código", "Descripción" };
            int[] AnchoCol = { 80, 350 };
            Ayuda.Nombre_BD_Datos = CnnFalp.DBNombre;
            Ayuda.Pass = CnnFalp.DBPass;
            Ayuda.User = CnnFalp.DBUser;
            Ayuda.TipoBase = 1;
            Ayuda.NombreColumnas = NomCol;
            Ayuda.AnchoColumnas = AnchoCol;
            Ayuda.TituloConsulta = "Seleccionar Etapa";
            Ayuda.Package = PCK;
            Ayuda.Procedimiento = "P_CARGA_ETAPA";
            Ayuda.Generar_ParametroBD("PIN_GP_ID", txtPatologia.Tag, DbType.Int64, ParameterDirection.Input);
            Ayuda.EjecutarSql();
        }

        private void btnSubEtapa_Click(object sender, EventArgs e)
        {
            txtSubEtapa.Text = string.Empty;
            txtSubEtapa.Tag = string.Empty;
            Cargar_Sub_Etapa();
        }

        private void TraeDBSubEtapa(ref AyudaSpreadNet.AyudaSprNet Ayuda, string descripcion)
        {
            string[] NomCol = { "Código", "Descripción" };
            int[] AnchoCol = { 80, 350 };
            Ayuda.Nombre_BD_Datos = CnnFalp.DBNombre;
            Ayuda.Pass = CnnFalp.DBPass;
            Ayuda.User = CnnFalp.DBUser;
            Ayuda.TipoBase = 1;
            Ayuda.NombreColumnas = NomCol;
            Ayuda.AnchoColumnas = AnchoCol;
            Ayuda.TituloConsulta = "Seleccionar Sub Etapa";
            Ayuda.Package = PCK;
            Ayuda.Procedimiento = "P_CARGA_SUB_ETAPA";
            Ayuda.Generar_ParametroBD("PIN_GP_ID", txtPatologia.Tag, DbType.Int64, ParameterDirection.Input);
            Ayuda.Generar_ParametroBD("PIN_ETAPA", txtEtapa.Tag, DbType.Int64, ParameterDirection.Input);
            Ayuda.EjecutarSql();
        }

        private void btnPaquete_Click(object sender, EventArgs e)
        {
            Cargar_Paquete();
            if (Validar_paquete())
            {
               
                if (txtPaquete.Text != "")
                {
                    txtDiasVig.Focus();
                }
                else
                {
                    txtPaquete.Focus();
                }
            }

            else
            {
                MessageBox.Show("Estimado Usuario, El Paquete no esta asociado con esa Institución, por Favor Comunicarse con el área Comercial, para su vinculación  ", "Informacion Sistema", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtPaquete.Text = "";
                txtPaquete.Tag = "";
            }
        }

        private void TraeDBPquete(ref AyudaSpreadNet.AyudaSprNet Ayuda)
        {
            string[] NomCol = { "Código", "Descripción" };
            int[] AnchoCol = { 80, 350 };
            Ayuda.Nombre_BD_Datos = CnnFalp.DBNombre;
            Ayuda.Pass = CnnFalp.DBPass;
            Ayuda.User = CnnFalp.DBUser;
            Ayuda.TipoBase = 1;
            Ayuda.NombreColumnas = NomCol;
            Ayuda.AnchoColumnas = AnchoCol;
            Ayuda.TituloConsulta = "Seleccionar Paquete";
            Ayuda.Package = PCK;
            Ayuda.Procedimiento = "P_CARGA_paquete";
            Ayuda.Generar_ParametroBD("PIN_GP_ID", txtPatologia.Tag, DbType.Int64, ParameterDirection.Input);
            Ayuda.Generar_ParametroBD("PIN_ETAPA", txtEtapa.Tag, DbType.Int64, ParameterDirection.Input);
            Ayuda.Generar_ParametroBD("PIN_SUB_ETAPA", txtSubEtapa.Tag, DbType.Int64, ParameterDirection.Input);
            Ayuda.EjecutarSql();
        }

        private void btnRespaldo_Click(object sender, EventArgs e)
        {
            txtRespaldo.Text = string.Empty;
            txtRespaldo.Tag = string.Empty;
            Cargar_Respaldo();
        }

        private void TraeDBDocumento(ref AyudaSpreadNet.AyudaSprNet Ayuda,string descripcion)
        {
            string[] NomCol = { "Código", "Descripción" };
            int[] AnchoCol = { 80, 350 };
            Ayuda.Nombre_BD_Datos = CnnFalp.DBNombre;
            Ayuda.Pass = CnnFalp.DBPass;
            Ayuda.User = CnnFalp.DBUser;
            Ayuda.TipoBase = 1;
            Ayuda.NombreColumnas = NomCol;
            Ayuda.AnchoColumnas = AnchoCol;
            Ayuda.TituloConsulta = "Seleccionar Documento  ";
            Ayuda.Package = PCK;
            Ayuda.Procedimiento = "P_CARGA_PARAM_GRALES";
            Ayuda.Generar_ParametroBD("PIN_CODIGO", 5, DbType.Int64, ParameterDirection.Input);
            Ayuda.Generar_ParametroBD("PIN_CODIGO", descripcion, DbType.String, ParameterDirection.Input);
            Ayuda.EjecutarSql();
        }

        private void txtRecepcion_ValueChanged(object sender, EventArgs e)
        {
            string fec = txtDiasVig.Text == "" ? fec = "0" : fec = txtDiasVig.Text;
            txtFecTermino.Text = txtIniGES.Value.AddDays(Convert.ToDouble(fec)).ToShortDateString();

            DateTime fecha1 = Convert.ToDateTime(txtFecTermino.Text);
            DateTime fecha2 = Convert.ToDateTime(txtRecepcion.Text);

            if (Convert.ToDateTime(txtIniGES.Text) <= fecha2)
            {
                TimeSpan dias = fecha1.Subtract(fecha2);
                txtDiasRestantes.Text = dias.Days.ToString();
            }
            else
            {
               MessageBox.Show("Estimado Usuario, Fecha de Recepcion no puede ser menor ", "Informacion Sistema", MessageBoxButtons.OK, MessageBoxIcon.Information);
               txtRecepcion.Text = txtIniGES.Text;
            }
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            DialogResult opc = MessageBox.Show("Estimado Usuario,¿Esta Seguro de Agregar esta Patologia " + txtPatologia.Text + "", "Informacion", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

              if (opc == DialogResult.Yes)
              {
                  try
                  {
                     
                          if (Validar_patologia())
                          {
                              DataRow NewFila = Tbl_Caso.NewRow();
                              NewFila["ID_FILA"] = Tbl_Caso.Rows.Count + 1;
                              NewFila["ID_PATOLOGIA"] = txtPatologia.Tag;
                              NewFila["DESC_PATOLOGIA"] = txtPatologia.Text;
                              NewFila["ID_ETAPA"] = txtEtapa.Tag;
                              NewFila["DESC_ETAPA"] = txtEtapa.Text;
                              NewFila["ID_SUB_ETAPA"] = txtSubEtapa.Tag;
                              NewFila["DESC_SUB_ETAPA"] = txtSubEtapa.Text;
                              NewFila["ID_PAQUETE"] = txtPaquete.Tag;
                              NewFila["DESC_PAQUETE"] = txtPaquete.Text;
                              NewFila["DIAS"] = txtDiasVig.Text.Equals(string.Empty) ? "0" : txtDiasVig.Text;
                              NewFila["FECHA_INICIO"] = txtIniGES.Text;
                              NewFila["FECHA_RECEPCION"] = txtRecepcion.Text;
                              NewFila["OBSERVACION"] = "";
                              Tbl_Caso.Rows.Add(NewFila);

                              Gv_Casos.AutoGenerateColumns = false;
                              Gv_Casos.DataSource = Tbl_Caso;
                              btnGrabarGes.Enabled = true;
                              //    btnGrabarGes.Enabled = true;
                              //    btnAgregar.Enabled = false;

                              //    MessageBox.Show("Estimado Usuario, Fue Agregada la Información Correctamente ", "Informacion Sistema", MessageBoxButtons.OK, MessageBoxIcon.Information);
                          }

 
                  }
                  catch (Exception ex)
                  {
                  
                      MessageBox.Show(ex.Message, "Error al intentar grabar ");
                  }
              }
             // LimpiarTxt(gr_dato);

              txtEtapa.Text = "";
              txtEtapa.Tag = "";
              txtSubEtapa.Text = "";
              txtSubEtapa.Tag = "";
              txtPaquete.Text = "";
              txtPaquete.Tag = "";
              LimpiarTxt(groupBox1);
       
      
              txtDiasRestantes.Text = "";
              txtFecTermino.Text = "";
             // gr_dato.Enabled = false;
            //  groupBox1.Enabled = false;
              txtPaquete.Enabled = false;
              btnPaquete.Enabled = false;
              txtSubEtapa.Enabled = false;
              btnSubEtapa.Enabled = false;
              txtEtapa.Focus();
        }

        private Boolean Validar_paquete()
        {
            Boolean var = false;

            DataTable dt = new DataTable();
            if (CnnFalp.Estado == ConnectionState.Closed) CnnFalp.Abrir();

            CnnFalp.CrearCommand(CommandType.StoredProcedure, PCK + ".P_VALIDAR_PREV_PAQUETE");
            CnnFalp.ParametroBD("PIN_PAQUETE", Convert.ToInt64(txtPaquete.Tag), DbType.Int64, ParameterDirection.Input);
            CnnFalp.ParametroBD("PIN_PREVISION", Convert.ToInt64(txtPrevision.Tag), DbType.Int64, ParameterDirection.Input);
           
            dt.Load(CnnFalp.ExecuteReader());

            if (dt.Rows.Count > 0)
            {
                var = true;
            }

            return var;
        }

        private Boolean Validar_patologia()
        {
            Boolean var = false;

            if (txtPatologia.Tag != "")
            {
                if (txtEtapa.Tag != "")
                {
                    if (txtSubEtapa.Tag != "")
                    {
                        if (txtPaquete.Tag != "")
                        {
                           
                                int cont = 0;
                                foreach (DataRow fila3 in Tbl_Caso.Select(" ID_ETAPA= '" + Convert.ToInt32(txtEtapa.Tag) + "' and  ID_SUB_ETAPA ='" + Convert.ToInt32(txtSubEtapa.Tag) + "' AND  ID_PAQUETE= '" + Convert.ToInt32(txtPaquete.Tag) + "'"))
                                {
                                    cont++;
                                }

                                if (cont == 0)
                                {
                                    var = true;
                                }
                                else
                                {
                                    MessageBox.Show("Estimado Usuario, La Etapa, Sub-Etapa y Paquete ya se encuentra Registrada en la Lista", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                }
  
                        }
                        else
                        {
                            MessageBox.Show("Estimado Usuario, El Campo Paquete se encuentra Vacio ", "Informacion Sistema", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            txtPaquete.Focus();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Estimado Usuario, El Campo Sub Etapa se encuentra Vacio ", "Informacion Sistema", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        txtSubEtapa.Focus();
                    }
                }
                else
                {
                    MessageBox.Show("Estimado Usuario, El Campo Etapa se encuentra Vacio ", "Informacion Sistema", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtEtapa.Focus();
                }
            }
            else {
                MessageBox.Show("Estimado Usuario, El Campo Patologia se encuentra Vacio ", "Informacion Sistema", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtPatologia.Focus();
            }


            return var;
        }

        private void btnGrabarGes_Click(object sender, EventArgs e)
        {
              DialogResult opc = MessageBox.Show("Estimado Usuario,¿Esta Seguro de Modificar esta Patología " + txtPatologia.Text + "", "Informacion", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

              if (opc == DialogResult.Yes)
              {
                  CnnFalp.IniciarTransaccion();
                  try
                  {
                      CrearCaso();
                      Crear_DetalleCaso();
                      CnnFalp.ConfirmarTransaccion();
                      Limpiar();
                      MessageBox.Show("Estimado Usuario, Fue Grabado Correctamente la Información ", "Informacion Sistema", MessageBoxButtons.OK, MessageBoxIcon.Information);


                  }
                  catch (Exception ex)
                  {
                      CnnFalp.ReversarTransaccion();
                      MessageBox.Show(ex.Message, "Error al intentar grabar ");
                  }
              }
        }

        private void CrearCaso()
        {
            
                CnnFalp.CrearCommand(CommandType.StoredProcedure, PCK + ".P_INSERTA_CASO");
                CnnFalp.ParametroBD("PIN_CORRELATIVO", txtFicha.Tag, DbType.Int64, ParameterDirection.Input);
                CnnFalp.ParametroBD("PIN_SEXO", txtSexo.Text, DbType.String, ParameterDirection.Input);
                CnnFalp.ParametroBD("PIN_EDAD", txtEdad.Text, DbType.Int64, ParameterDirection.Input);
                CnnFalp.ParametroBD("PIN_TIPO_PREVISION", txtTipoPrev.Tag, DbType.Int64, ParameterDirection.Input);
                CnnFalp.ParametroBD("PIN_PREVISION", txtPrevision.Tag, DbType.Int64, ParameterDirection.Input);
                CnnFalp.ParametroBD("PIN_PLAN", txtPlanPrev.Tag, DbType.Int64, ParameterDirection.Input);
                CnnFalp.ParametroBD("PIN_USER", Db_Usuario, DbType.String, ParameterDirection.Input);
                CnnFalp.ParametroBD("PIN_DERIVADOR", txtDerivador.Tag, DbType.Int64, ParameterDirection.Input);
                CnnFalp.ParametroBD("PIN_TIPO_DOC", txtRespaldo.Tag, DbType.Int64, ParameterDirection.Input);
                CnnFalp.ParametroBD("PIN_NUMERO_DOC", txtDocRespaldo.Text, DbType.String, ParameterDirection.Input);
                CnnFalp.ParametroBD("PIN_PATOLOGIA", txtPatologia.Tag, DbType.Int64, ParameterDirection.Input);
      
                

                CnnFalp.ParametroBD("POUT_ID", 0, DbType.Int64, ParameterDirection.Output);
                CnnFalp.ExecuteNonQuery();
         

            v_ID = Convert.ToInt64(CnnFalp.ParamValue("POUT_ID").ToString());


        }

        private void Crear_DetalleCaso()
        {

            foreach(DataRow Tc in Tbl_Caso.Rows)
            {
                CnnFalp.CrearCommand(CommandType.StoredProcedure, PCK + ".P_INSERTA_DETALLE");
                CnnFalp.ParametroBD("PIN_GC_ID", v_ID, DbType.Int64, ParameterDirection.Input);
                CnnFalp.ParametroBD("PIN_ETAPA", Tc["ID_ETAPA"], DbType.Int64, ParameterDirection.Input);
                CnnFalp.ParametroBD("PIN_SUB_ETAPA", Tc["ID_SUB_ETAPA"], DbType.Int64, ParameterDirection.Input);
                CnnFalp.ParametroBD("PIN_PAQUETE", Tc["ID_PAQUETE"], DbType.Int64, ParameterDirection.Input);
                CnnFalp.ParametroBD("PIN_DIAS", Tc["DIAS"], DbType.Int64, ParameterDirection.Input);
                CnnFalp.ParametroBD("PIN_FECHA_INICIO", Tc["FECHA_INICIO"], DbType.String, ParameterDirection.Input);
                CnnFalp.ParametroBD("PIN_FECHA_RECEPCION", Tc["FECHA_RECEPCION"], DbType.String, ParameterDirection.Input);
                CnnFalp.ParametroBD("PIN_USER", Db_Usuario, DbType.String, ParameterDirection.Input);
                CnnFalp.ParametroBD("PIN_OBSERVACION", v_observacion, DbType.String, ParameterDirection.Input);
                CnnFalp.ExecuteNonQuery();
           }
            
        }

        private void Limpiar()
        {
            AsIgnaTag();
            //grpPaciente
            //grpDatosGES
            //groupBox2
            //groupBox1
            LimpiarTxt(grpPaciente);
            LimpiarTxt(grpDatosGES);
            txtRecepcion.Text=txtIniGES.Text;
            Tbl_Caso.Clear();
            Gv_Casos.AutoGenerateColumns = false;
            Gv_Casos.DataSource = Tbl_Caso;
            group_datos.Enabled = false;
            btnGrabarGes.Enabled = false;
            LimpiarTxt(gr_dato);
            LimpiarTxt(groupBox1);
            txtDiasRestantes.Text = "";
            txtFecTermino.Text = "";
            grpDatosGES.Enabled = false;

        }

        private void LimpiarTxt(Control Crontrol)
        {
            foreach (Control c in Crontrol.Controls)
            {
                if (c is TextBox)
                {
                    c.Text = string.Empty;
                    c.Tag = string.Empty;
                }
            }
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
             DialogResult opc = MessageBox.Show("Estimado Usuario,¿Esta Seguro de  Limpiar los campos de Pantalla?", "Informacion", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

             if (opc == DialogResult.Yes)
             {
                 Limpiar();
             }
        }


   //  AGREGAR METODOS  30-05-2018


        private void txtDerivador_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(char.IsLetter(e.KeyChar)) && (e.KeyChar != (char)Keys.Back) && (e.KeyChar != (char)Keys.Enter) && (e.KeyChar != (char)Keys.Enter) && (e.KeyChar != (char)Keys.Space))
            {
                e.Handled = true;

                return;
            }
            else
            {
               
                if (e.KeyChar == (char)13)
                {
                    Cargar_Derivador();
                    if (txtDerivador.Text != "")
                    {
                        txtRespaldo.Focus();
                    }
                    else
                    {
                        txtDerivador.Focus();
                    }
                }
            }
        }

        private void txtRespaldo_KeyPress(object sender, KeyPressEventArgs e)
      {
          if (!(char.IsLetter(e.KeyChar)) && (e.KeyChar != (char)Keys.Back) && (e.KeyChar != (char)Keys.Enter) && (e.KeyChar != (char)Keys.Space))
            {
                e.Handled = true;

                return;
            }
            else
            {
                
                if (e.KeyChar == (char)13)
                {
                    Cargar_Respaldo();
                    if (txtRespaldo.Text != "")
                    {
                        txtDocRespaldo.Focus();
                    }
                    else
                    {
                        txtRespaldo.Focus();
                    }
                }
            }
        }

        private void txtDocRespaldo_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(char.IsNumber(e.KeyChar)) && (e.KeyChar != (char)Keys.Back) && (e.KeyChar != (char)Keys.Enter))
            {
                e.Handled = true;

                return;
            }
            else
            {

                if (e.KeyChar == (char)13)
                {
                 
                        if (txtDocRespaldo.Text != "")
                        {
                            if (!Validar_doc_respaldo())
                            {                              
                                group_datos.Enabled = true;
                               // gr_dato.Enabled = true;
                                groupBox1.Enabled = true;                      
                                grpDatosGES.Enabled = false;
                                txtPatologia.Enabled = true;
                                btnPatologia.Enabled = true;
                                txtPaquete.Enabled = false;
                                btnPaquete.Enabled = false;
                                txtSubEtapa.Enabled = false;
                                btnSubEtapa.Enabled = false;
                                txtEtapa.Enabled = false;
                                btnEtapa.Enabled = false;
                                txtPatologia.Focus();
                            }
                            else
                            {
                                MessageBox.Show("Estimado Usuario, La información Ingresada ya Existe ", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);        
                                group_datos.Enabled = false;
                                txtDocRespaldo.Text = "";
                            }
                        }
                   
                }
            }
        }

        private void Cargar_Derivador()
        {
           TraeDerivador(ref ayudaGES, txtDerivador.Text.ToUpper());
            if (!ayudaGES.EOF())
            {
                txtDerivador.Tag = ayudaGES.Fields(1);
                txtDerivador.Text = ayudaGES.Fields(2);
                txtRespaldo.Enabled = true;
                btnRespaldo.Enabled = true;
                txtRespaldo.Focus();

            }
        }

        private void Cargar_Respaldo()
        
        {
            TraeDBDocumento(ref ayudaGES,txtRespaldo.Text);

            if (!ayudaGES.EOF())
            {
                txtRespaldo.Tag = ayudaGES.Fields(0);
                txtRespaldo.Text = ayudaGES.Fields(1);
                txtDocRespaldo.Enabled = true;
                txtDocRespaldo.Focus();
            }
        }

        private void Cargar_Patalogia()
        {
            TraeDBParam(ref ayudaGES, 81, txtPatologia.Text.ToUpper());
            if (!ayudaGES.EOF())
            {
                txtPatologia.Tag = Convert.ToInt64(ayudaGES.Fields(0));
                txtPatologia.Text = ayudaGES.Fields(1);
                btnEtapa.Enabled = true;
                txtEtapa.Enabled = true;
                txtEtapa.Focus();
            }
        }

        private void Cargar_Etapa()
        {
            TraeDBEtapa(ref ayudaGES, txtEtapa.Text.ToUpper());

            if (!ayudaGES.EOF())
            {
                txtEtapa.Tag = ayudaGES.Fields(0);
                txtEtapa.Text = ayudaGES.Fields(1);
                Cargar_Sub_Etapa();
                btnSubEtapa.Enabled = true;
                txtSubEtapa.Enabled = true;
                txtPaquete.Focus();
            }
            else
            {

                txtSubEtapa.Focus();
            }
           
        }

        private void Cargar_Sub_Etapa()
        {
            TraeDBSubEtapa(ref ayudaGES, txtEtapa.Text.ToUpper());
            if (!ayudaGES.EOF())
            {
                txtSubEtapa.Tag = ayudaGES.Fields(0);
                txtSubEtapa.Text = ayudaGES.Fields(1);
                btnPaquete.Enabled = true;
                txtPaquete.Enabled = true;
                txtPaquete.Focus();
            }
        }

        private void Cargar_Paquete()
        {

            TraeDBPquete(ref ayudaGES);
            if (!ayudaGES.EOF())
            {
                txtPaquete.Tag = ayudaGES.Fields(0);
                txtPaquete.Text = ayudaGES.Fields(1);
                txtDiasVig.Text = ayudaGES.Fields(2).Equals(string.Empty) ? "0" : ayudaGES.Fields(2);
                 txtFecTermino.Text = txtRecepcion.Value.AddDays(Convert.ToDouble(txtDiasVig.Text)).ToShortDateString();

                DateTime fecha1 = Convert.ToDateTime(txtFecTermino.Text);
                DateTime fecha2 = Convert.ToDateTime(txtRecepcion.Text);

                TimeSpan dias = fecha1.Subtract(fecha2);

                txtDiasRestantes.Text = dias.Days.ToString();


                txtIniGES.Enabled = true;
                txtRecepcion.Enabled = true;
                btnDerivador.Enabled = true;
                txtRespaldo.Enabled = true;
                btnRespaldo.Enabled = true;
                txtIniGES.CustomFormat = "dd/MM/yyyy";
                txtRecepcion.CustomFormat = "dd/MM/yyyy";
                btnAgregar.Focus();
            }
        }

        private Boolean Validar_doc_respaldo(){
        
            Boolean var = false;
            DataTable dt = new DataTable();
            if (CnnFalp.Estado == ConnectionState.Closed) CnnFalp.Abrir();

            CnnFalp.CrearCommand(CommandType.StoredProcedure, PCK + ".P_VALIDAR_DOC_RESPALDO");
            CnnFalp.ParametroBD("PIN_DERIVADOR", Convert.ToInt64(txtDerivador.Tag), DbType.Int64, ParameterDirection.Input);
            CnnFalp.ParametroBD("PIN_RESPALDO", Convert.ToInt64(txtRespaldo.Tag), DbType.Int64, ParameterDirection.Input);
            CnnFalp.ParametroBD("PIN_DOC_RESPALDO", Convert.ToInt64(txtDocRespaldo.Text), DbType.Int64, ParameterDirection.Input);


            dt.Load(CnnFalp.ExecuteReader());

            if (dt.Rows.Count>0)
             {
                 var = true;
             }


            return var;
        }


        private Boolean Validar_estado_caso(Int64 correlativo)
        {

            Boolean var = false;
            DataTable dt = new DataTable();
            if (CnnFalp.Estado == ConnectionState.Closed) CnnFalp.Abrir();

            CnnFalp.CrearCommand(CommandType.StoredProcedure, PCK + ".P_VALIDAR_CASO_GES");
            CnnFalp.ParametroBD("PIN_CORRELATIVO", correlativo, DbType.Int64, ParameterDirection.Input);
            dt.Load(CnnFalp.ExecuteReader());
         
            foreach (DataRow miRow1 in dt.Rows)
            {
                val_caso = miRow1["ESTADO"].ToString(); 
            }

            if (val_caso == "S")
             {
                 var = true;
             }
            return var;
        }

        private void  Bloqueo_Datos_Generales()
        {
            txtRespaldo.Enabled=true;
            btnRespaldo.Enabled=true;
            txtDocRespaldo.Enabled=false;
            txtRespaldo.Focus();
        }

      

        private void txtPatologia_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(char.IsLetter(e.KeyChar)) && (e.KeyChar != (char)Keys.Back) && (e.KeyChar != (char)Keys.Enter) && (e.KeyChar != (char)Keys.Space))
            {
                e.Handled = true;

                return;
            }
            else
            {

                if (e.KeyChar == (char)13)
                {
                    Cargar_Patalogia();
                    if (txtPatologia.Text != "")
                    {

                        txtPatologia.Enabled = false;
                        btnPatologia.Enabled = false;
                        txtEtapa.Focus();
                        txtPaquete.Enabled = false;
                        btnPaquete.Enabled = false;
                        txtSubEtapa.Enabled = false;
                        btnSubEtapa.Enabled = false;
                    }
                    else
                    {
                        txtPatologia.Focus();
                    }
                }
                else
                {
                    txtPaquete.Enabled = false;
                    btnPaquete.Enabled = false;
                    txtSubEtapa.Enabled = false;
                    btnSubEtapa.Enabled = false;
                    txtEtapa.Enabled = false;
                    btnEtapa.Enabled = false;
                    txtPatologia.Focus();
                }
            }
        }

        private void txtEtapa_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(char.IsLetter(e.KeyChar)) && (e.KeyChar != (char)Keys.Back) && (e.KeyChar != (char)Keys.Enter) && (e.KeyChar != (char)Keys.Space))
            {
                e.Handled = true;

                return;
            }
            else
            {

                if (e.KeyChar == (char)13)
                {
                    Cargar_Etapa();
                    if (txtEtapa.Text != "")
                    {
                        txtPaquete.Focus();
                    }
                    else
                    {
                        txtEtapa.Focus();
                    }
                }
            }
        }

        private void txtSubEtapa_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(char.IsLetter(e.KeyChar)) && (e.KeyChar != (char)Keys.Back) && (e.KeyChar != (char)Keys.Enter) && (e.KeyChar != (char)Keys.Space))
            {
                e.Handled = true;

                return;
            }
            else
            {

                if (e.KeyChar == (char)13)
                {
                    if (txtSubEtapa.Tag == "")
                    {
                        Cargar_Sub_Etapa();
                     
                    }
                    if (txtSubEtapa.Tag != "")
                    {
                      
                        txtPaquete.Focus();
                    }
                    else
                    {
                        txtSubEtapa.Focus();
                    }
                }
            }
        }

        private void txtPaquete_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(char.IsLetter(e.KeyChar)) && (e.KeyChar != (char)Keys.Back) && (e.KeyChar != (char)Keys.Enter) && (e.KeyChar != (char)Keys.Space))
            {
                e.Handled = true;

                return;
            }
            else
            {

                if (e.KeyChar == (char)13)
                {
                    Cargar_Paquete();
                    if (Validar_paquete())
                    {
                       
                        if (txtPaquete.Text != "")
                        {
                            txtDiasVig.Focus();
                        }
                        else
                        {
                            txtPaquete.Focus();
                        }
                    }

                    else
                    {
                        MessageBox.Show("Estimado Usuario, El Paquete no esta asociado con esa Institución, por Favor Comunicarse con el área Comercial, para su vinculación  ", "Informacion Sistema", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        txtPaquete.Text = "";
                        txtPaquete.Tag = "";
                    }
                   
                }
            }
        }

        private void Gv_Casos_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {

            if (e.RowIndex > -1)
            {
                if (e.ColumnIndex == 0)
                {
                string nom = Gv_Casos.Rows[e.RowIndex].Cells["NOM_PATOLOGIA"].Value.ToString();
                     DialogResult opc = MessageBox.Show("Estimado Usuario,¿Esta Seguro de Eliminar esta Patología " + nom + "", "Información", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                     if (opc == DialogResult.Yes)
                     {
                         Gv_Casos.Rows.RemoveAt(Gv_Casos.CurrentRow.Index);
                         MessageBox.Show("Estimado Usuario, Fue Eliminada Correctamente la Patología  " + nom + " ", "Información Sistema", MessageBoxButtons.OK, MessageBoxIcon.Information);
                       //  LimpiarTxt(gr_dato);

                         LimpiarTxt(groupBox1);
                         btnAgregar.Enabled = true;
                     }

                }

                else{
                if (e.ColumnIndex == 1)
                {
                    DialogResult opc = MessageBox.Show("Estimado Usuario,¿Esta Seguro de Ingresar una Observación", "Información", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (opc == DialogResult.Yes)
                    {
                        int cod = Convert.ToInt32(Gv_Casos.Rows[e.RowIndex].Cells["ID_FILA"].Value.ToString());

                        string v_coment = extraer_comentario(cod);
                        Frm_Agregar_Observacion frm = new Frm_Agregar_Observacion(v_coment);
                        frm.ShowDialog();
                        v_observacion = frm.OBSERVACION;
                        Agregar_comentario(cod, v_observacion);
                    }
                 }
               }
            }
        }


        private string extraer_comentario(int cod)
        {
            string var = "";

            foreach (DataRow miRow1 in Tbl_Caso.Select("ID_FILA=" + cod))
            {
                   var= miRow1["OBSERVACION"].ToString();
            }

            Tbl_Caso.AcceptChanges();

            return var;
        }

        private void Agregar_comentario(int cod, string comentario)
        {
            string var = "";

            foreach (DataRow miRow1 in Tbl_Caso.Select("ID_FILA=" + cod))
            {
                 miRow1["OBSERVACION"]=comentario;
            }

            Tbl_Caso.AcceptChanges();
        }

        private void CambiarBlanco_TextLeave(object sender, EventArgs e)
        {
            TextBox GB = (TextBox)sender;
            GB.BackColor = Color.White;

        }

        private void CambiarColor_TextEnter(object sender, EventArgs e)
        {
            TextBox GB = (TextBox)sender;
            GB.BackColor = Color.FromArgb(255, 224, 192);
        }

        private void CambiarColor_Enter(object sender, EventArgs e)
        {
            GroupBox GB = (GroupBox)sender;
            GB.BackColor = Color.FromArgb(255, 255, 192);
        }

        private void CambiarBlanco_Leave(object sender, EventArgs e)
        {
            GroupBox GB = (GroupBox)sender;
            GB.BackColor = Color.WhiteSmoke;
        }

        private void txtDiasVig_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                string fec = txtDiasVig.Text == "" ? fec = "0" : fec = txtDiasVig.Text;
                txtFecTermino.Text = txtIniGES.Value.AddDays(Convert.ToDouble(fec)).ToShortDateString();

                DateTime fecha1 = Convert.ToDateTime(txtFecTermino.Text);
                DateTime fecha2 = Convert.ToDateTime(txtRecepcion.Text);

                if (Convert.ToDateTime(txtIniGES.Text) <= fecha2)
                {
                    TimeSpan dias = fecha1.Subtract(fecha2);
                    txtDiasRestantes.Text = dias.Days.ToString();
                }
                btnAgregar.Focus();
            }
        }

        private void txtIniGES_ValueChanged(object sender, EventArgs e)
        {
            if (Convert.ToDateTime(txtIniGES.Text) > Convert.ToDateTime(txtRecepcion.Text))
            {
                MessageBox.Show("Estimado Usuario, Fecha de Inicio no puede ser mayor Fecha de Recepción ", "Información Sistema", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtIniGES.Text = txtRecepcion.Text;
                string fec = txtDiasVig.Text == "" ? fec = "0" : fec = txtDiasVig.Text;
                txtFecTermino.Text = txtIniGES.Value.AddDays(Convert.ToDouble(fec)).ToShortDateString();

                DateTime fecha1 = Convert.ToDateTime(txtFecTermino.Text);
                DateTime fecha2 = Convert.ToDateTime(txtRecepcion.Text);

                if (Convert.ToDateTime(txtIniGES.Text) <= fecha2)
                {
                    TimeSpan dias = fecha1.Subtract(fecha2);
                    txtDiasRestantes.Text = dias.Days.ToString();
                }
            }
        }

        private void btn_limpiar_patologia_Click(object sender, EventArgs e)
        {
             DialogResult opc = MessageBox.Show("Estimado Usuario,¿Esta Seguro de  Limpiar los campos de la Patología?", "Informacion", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

             if (opc == DialogResult.Yes)
             {
                // LimpiarTxt(gr_dato);
                 txtEtapa.Text = "";
                 txtEtapa.Tag = "";
                 txtSubEtapa.Text = "";
                 txtSubEtapa.Tag = "";
                 txtPaquete.Text = "";
                 txtPaquete.Tag = "";
                 LimpiarTxt(groupBox1);
                 txtDiasRestantes.Text = "";
                 txtFecTermino.Text = "";
                 txtPaquete.Enabled = false;
                 btnPaquete.Enabled = false;
                 txtSubEtapa.Enabled = false;
                 btnSubEtapa.Enabled = false;
             }
        }

        private void Gv_Casos_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                e.PaintBackground(e.ClipBounds, false);
                Font drawFont = new Font("Trebuchet MS", 8, FontStyle.Bold);
                SolidBrush drawBrush = new SolidBrush(Color.White);
                StringFormat StrFormat = new StringFormat();
                StrFormat.Alignment = StringAlignment.Center;
                StrFormat.LineAlignment = StringAlignment.Center;

                e.Graphics.DrawImage(Properties.Resources.HeaderGV, e.CellBounds);
                e.Graphics.DrawString(Gv_Casos.Columns[e.ColumnIndex].HeaderText, drawFont, drawBrush, e.CellBounds, StrFormat);

                e.Handled = true;
                drawBrush.Dispose();
            }
        }

    }
      
}