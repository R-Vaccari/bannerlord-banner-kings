<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:output omit-xml-declaration="yes"/>
    <xsl:template match="@*|node()">
        <xsl:copy>
            <xsl:apply-templates select="@*|node()"/>
        </xsl:copy>
    </xsl:template>


    <xsl:template match="Culture[@id='empire']/clan_names">
        <clan_names>
		  <name name="Acapanos" />
		  <name name="Angarys" />
		  <name name="Balastisos" />
		  <name name="Corstases" />
		  <name name="Delicos" />
		  <name name="Elysos" />
		  <name name="Gessios" />
		  <name name="Jalos" />
		  <name name="Lestharos" />
		  <name name="Meones" />
		  <name name="Nathanys" />
		  <name name="Opynates" />
		  <name name="Paltos" />
		  <name name="Phenigos" />
		  <name name="Stracanasthes" />
		  <name name="Sumessos" />
		  <name name="Therycos" />
		  <name name="Zebales" />
		  <name name="Turullios" />
		  <name name="Tarquinos" />
		  <name name="Viatores" />
		  <name name="Zosimus" />
		  <name name="Viselzes" />
		  <name name="Septemezes" />
		  <name name="Nepioes" />
		  <name name="Marcallas" />
		  <name name="Caeles" />
		  <name name="Pinaries" />
		  <name name="Serranetzes" />
		  <name name="Sylvianes" />
		  <name name="Paltos" />
		  <name name="Parnesetzes" />
		  <name name="Cordios" />
		  <name name="Cossos" />
		  <name name="Sepurcius" />
		  <name name="Ausonioes" />
		  <name name="Collinos" />
		  <name name="Aniensis" />
		  <name name="Velines" />
		  <name name="Voturos" />
		  <name name="Scaptes" />
		  <name name="Romilis" />
		  <name name="Amates" />
		  <name name="Rhagabes" />
		  <name name="Tzimiskes" />
		  <name name="Phoca" />
		  <name name="Glycas" />
		  <name name="Kerularios" />
		  <name name="Stauricies" />
		  <name name="Vatatzes" />
		  <name name="Zimisces" />
		  <name name="Lascaris" />
		  <name name="Bardanes" />
		  <name name="Zonaras" />
		  <name name="Ducas" />
		  <name name="Maniakes" />
		  <name name="Lecapenas" />
		  <name name="Rhangabes" />
		  <name name="Laonicos" />
		  <name name="Vitalianos" />
		  <name name="Alypes" />
		  <name name="Basiles" />
        </clan_names>
    </xsl:template>
	
	<xsl:template match="Culture[@id='aserai']/clan_names">
        <clan_names>
		  <name name="Bani Aska" />
		  <name name="Bani Dhamin" />
		  <name name="Bani Fasus" />
		  <name name="Bani Julul" />
		  <name name="Bani Kinyan" />
		  <name name="Bani Laikh" />
		  <name name="Bani Mushala" />
		  <name name="Bani Nir" />
		  <name name="Bani Tharuq" />
		  <name name="Bani Yatash" />
		  <name name="Bani Zus" />
		  <name name="Bani Nadir" />
		  <name name="Bani Qainuqa" />
		  <name name="Bani Bakr" />
		  <name name="Bani Tamim" />
		  <name name="Bani Ghatafan" />
		  <name name="Bani Hawazin" />
		  <name name="Bani Abdul" />
		  <name name="Bani Qays" />
		  <name name="Bani Madh'hij" />
		  <name name="Bani Kinanah" />
		  <name name="Bani Malik" />
		  <name name="Bani Lam" />
		  <name name="Bani Kilab" />
		  <name name="Bani Hasan" />
		  <name name="Bani Ali" />
		  <name name="Bani Shammar" />
		  <name name="Bani Mutair" />
		  <name name="Bani Duraji" />
        </clan_names>
    </xsl:template>
	
	<xsl:template match="Culture[@id='sturgia']/clan_names">
        <clan_names>
		  <name name="Asraloving" />
		  <name name="Boranoving" />
		  <name name="Folyoroving" />
		  <name name="Gendiroving" />
		  <name name="Iskanoving" />
		  <name name="Maregoving" />
		  <name name="Suratoving" />
		  <name name="Vyshoving" />
		  <name name="Yerchoving" />
		  <name name="Zhanoving" />
		  <name name="Dahloving" />
		  <name name="Volkoving" />
		  <name name="Kriskoving" />
		  <name name="Loftoving" />
		  <name name="Zelenkoving" />
		  <name name="Krogoving" />
		  <name name="Durnoving" />
		  <name name="Volloving" />
		  <name name="Burakoving" />
		  <name name="Falkoving" />
		  <name name="Ivanoving" />
		  <name name="Karsanoving" />
		  <name name="Lindoving" />
		  <name name="Malenkoving" />
		  <name name="Strandoving" />
		  <name name="Mitroving" />
		  <name name="Noktoving" />
		  <name name="Petroving" />
		  <name name="Curaving" />
		  <name name="Rivacheving" />
		  <name name="Nelaving" />
		  <name name="Redogving" />
		  <name name="Ismiroving" />
		  <name name="Mazeroving" />
		  <name name="Mechiving" />
		  <name name="Harisoving" />
		  <name name="Druliving" />
		  <name name="Rudinoving" />
		  <name name="Merigaving" />
		  <name name="Kaveloving" />
		  <name name="Gastyaving" />
		  <name name="Surdonoving" />
		  <name name="Hrusoving" />
		  <name name="Tehlrogoving" />
		  <name name="Menchinoving" />
		  <name name="Vladoving" />
        </clan_names>
    </xsl:template>
	
	<xsl:template match="Culture[@id='vlandia']/clan_names">
        <clan_names>
		  <name name="dey Sunor" />
		  <name name="dey Haringoth" />
		  <name name="dey Kelredan" />
		  <name name="dey Rindyar" />
		  <name name="dey Ryibelet" />
		  <name name="dey Tevarin" />
		  <name name="dey Tilbaut" />
		  <name name="dey Azgand" />
		  <name name="dey Balanli" />
		  <name name="dey Burglend" />
		  <name name="dey Ehlerdah" />
		  <name name="dey Emirind" />
		  <name name="dey Gisim" />
		  <name name="dey Ibirand" />
		  <name name="dey Nomarr" />
		  <name name="dey Rulund" />
		  <name name="dey Tosdhar" />
		  <name name="dey Veidarr" />
		  <name name="dey Yaliben" />
		  <name name="dey Yaragar" />
		  <name name="dey Amerre" />
		  <name name="dey Culmarr" />
		  <name name="dey Elberl" />
		  <name name="dey Ergellond" />
		  <name name="dey Dumarr" />
		  <name name="dey Serindiar" />
		  <name name="dey Etrosc" />
		  <name name="dey Velucand" />
		  <name name="dey Etrosc" />
		  <name name="dey Vorand" />		
		  <name name="dey Rodern" />		
		  <name name="dey Ilbeth" />		  
		  <name name="dey Tuvoren" />	
		  <name name="dey Harlend" />	
		  <name name="dey Norrigand" />	
		  <name name="dey Iveril" />
		  <name name="dey Mareven" />	
		  <name name="dey Iyindah" />		 
		  <name name="dey Dramug" />	
		  <name name="dey Reindi" />		  
		  <name name="dey Ibdeles" />		  
		  <name name="dey Istiniar" />		  
		  <name name="dey Pagundur" />		  
		  <name name="dey Kulund" />		  
		  <name name="dey Ruldi" />		  
		  <name name="dey Tadsamesh" />		  
		  <name name="dey Mechin" />		  
		  <name name="dey Rodrand" />		  
		  <name name="dey Venden" />		  
		  <name name="dey Heinrand" />		  
		  <name name="dey Aberan" />		  
		  <name name="dey Berden" />		  
		  <name name="dey Lovar" />		  
		  <name name="dey Maras" />		
		  <name name="dey Doucet" />		
		  <name name="dey Hamend" />		
		  <name name="dey Vachel" />		
		  <name name="dey Vyomont" />		
		  <name name="dey Bellamy" />		
		  <name name="dey Voill" />		
		  <name name="dey Verley" />		
		  <name name="dey Relin" />		
		  <name name="dey Monluc" />		
		  <name name="dey Corne" />		
		  <name name="dey Boivind" />		
		  <name name="dey Puchot" />		
		  <name name="dey Lefevbre" />		
		  <name name="dey Horbett" />		
		  <name name="dey Vullend" />		
		  <name name="dey Pane" />		
		  <name name="dey Challond" />		
		  <name name="dey Korevn" />	
		  <name name="dey Drover" />	
		  <name name="dey Douchot" />	
		  <name name="dey Parond" />
        </clan_names>
    </xsl:template>
	
	<xsl:template match="Culture[@id='battania']/clan_names">
        <clan_names>
		  <name name="fen Gwynn" />
		  <name name="fen Wiarannus" />
		  <name name="fen Gechal" />
		  <name name="fen Grudon" />
		  <name name="fen Kilywnn" />
		  <name name="fen Cryenwyn" />
		  <name name="fen Olahdog" />
		  <name name="fen Lewyn" />
		  <name name="fen Culain" />
		  <name name="fen Ifdywn" />
		  <name name="fen Ethrelog" />
		  <name name="fen Fethredt" />
		  <name name="fen Aellyn" />
		  <name name="fen Orain" />
		  <name name="fen Ilric" />
		  <name name="fen Kurewyn" />
		  <name name="fen Aleral" />
		  <name name="fen Eafraic" />
		  <name name="fen Ctrahaic" />
		  <name name="fen Lurain" />
		  <name name="fen Giswyn" />
		  <name name="fen Daethfec" />
		  <name name="fen Ilvaic" />
		  <name name="fen Kudolog" />
		  <name name="fen Fithwyn" />
		  <name name="fen Aertus" />
		  <name name="fen Brachar" />
		  <name name="fen Crusac" />
		  <name name="fen Domus" />
		  <name name="fen Earach" />
		  <name name="fen Fiachan" />
		  <name name="fen Loen" />
		  <name name="fen Morain" />
		  <name name="fen Seanel" />
		  <name name="fen Tuil" />
		  <name name="fen Asgaill" />
		  <name name="fen Céitinn" />
		  <name name="fen Laighin" />
		  <name name="fen Remicus" />
		  <name name="fen Audoti" />
		  <name name="fen Leucel" />
		  <name name="fen Adhugious" />
		  <name name="fen Scilain" />
		  <name name="fen Ambadus" />
		  <name name="fen Wirato" />
        </clan_names>
    </xsl:template>
	
	<xsl:template match="Culture[@id='khuzait']/clan_names">
        <clan_names>
		  <name name="Airit" />
		  <name name="Aldusunit" />
		  <name name="Bochit" />
		  <name name="Charait" />
		  <name name="Ingchit" />
		  <name name="Maranjit" />
		  <name name="Oranarit" />
		  <name name="Sunit" />
		  <name name="Tokhit" />
		  <name name="Ubchit" />
		  <name name="Yujit" />
		  <name name="Alchit" />
		  <name name="Kipchakit" />
		  <name name="Tarit" />
		  <name name="Alukhait" />
		  <name name="Yurkit" />
		  <name name="Bugunit" />
		  <name name="Belgunit" />
		  <name name="Merkit" />
		  <name name="Sakhait" />
		  <name name="Tarkhit" />
		  <name name="Khardakit" />  
		  <name name="Sandagit" />
		  <name name="Terbit" />
		  <name name="Kokachit" />
		  <name name="Yimekit" />
		  <name name="Tatarit" />
		  <name name="Namanit" />
		  <name name="Burulit" />  
		  <name name="Akadit" />
		  <name name="Asurit" />
		  <name name="Belirit" />
		  <name name="Nasugit" />
		  <name name="Sebulit" />
		  <name name="Ulusit" />
		  <name name="Tulugit" />  
		  <name name="Hugulit" />
		  <name name="Tiridit" />
		  <name name="Tunjit" />
		  <name name="Tansurit" />
		  <name name="Urumit" />
		  <name name="Zagusit" />
        </clan_names>
    </xsl:template>

</xsl:stylesheet>