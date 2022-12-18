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
        </clan_names>
    </xsl:template>
	
	<xsl:template match="Culture[@id='aserai']/clan_names">
        <clan_names>
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
        </clan_names>
    </xsl:template>

</xsl:stylesheet>