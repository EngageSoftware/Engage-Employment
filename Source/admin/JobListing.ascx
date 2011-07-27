<%@ Control Language="C#" Inherits="Engage.Dnn.Employment.Admin.JobListing" AutoEventWireup="false" CodeBehind="JobListing.ascx.cs" %>

<asp:GridView ID="JobsGrid" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="Normal employmentTable">
    <RowStyle CssClass="listingRow" />
    <AlternatingRowStyle CssClass="alternateListingRow" />
    <HeaderStyle CssClass="headerRow" />
    <EmptyDataTemplate><asp:Label runat="server" resourcekey="lblNoJobs" /></EmptyDataTemplate>
    <Columns>
        <asp:TemplateField>
            <HeaderStyle CssClass="locationHeader" />
            <ItemStyle CssClass="locationListing" />
            <HeaderTemplate>
                <asp:HyperLink runat="server" resourcekey="lnkLocationHeader" NavigateUrl='<%#EditUrl("ManageLocations") %>' /><%=this.Localize("LocationSeparator.Text") %><asp:HyperLink runat="server" resourcekey="lnkStateHeader" NavigateUrl='<%#EditUrl("ManageStates") %>' />
            </HeaderTemplate>
            <ItemTemplate>
                <span><%#HttpUtility.HtmlEncode(GetLocationName(Eval("LocationId"), Eval("LocationName"), Eval("StateName"), Eval("StateAbbreviation")))%></span>
            </ItemTemplate>
        </asp:TemplateField>
        <asp:TemplateField>
            <HeaderStyle CssClass="categoryHeader" />
            <ItemStyle CssClass="categoryListing" />
            <HeaderTemplate>
                <asp:HyperLink runat="server" resourcekey="lnkCategoryHeader" NavigateUrl='<%#EditUrl("ManageCategories") %>' />
            </HeaderTemplate>
            <ItemTemplate>
                <span><%#HttpUtility.HtmlEncode((string)Eval("CategoryName"))%></span>
            </ItemTemplate>
        </asp:TemplateField>
        <asp:TemplateField>
            <HeaderStyle CssClass="positionHeader" />
            <ItemStyle CssClass="positionListing" />
            <HeaderTemplate>
                <asp:HyperLink runat="server" resourcekey="lnkPositionHeader" NavigateUrl='<%#EditUrl("ManagePositions") %>' />
            </HeaderTemplate>
            <ItemTemplate>
                <span><%#HttpUtility.HtmlEncode((string)Eval("JobTitle"))%></span>
            </ItemTemplate>
        </asp:TemplateField>
        <asp:BoundField HeaderText="DatePosted" DataField="PostedDate">
            <HeaderStyle CssClass="datePostedHeader" />
            <ItemStyle CssClass="datePostedListing" />
        </asp:BoundField>
        <asp:BoundField HeaderText="StartDate" DataField="StartDate">
            <HeaderStyle CssClass="startDateHeader" />
            <ItemStyle CssClass="startDateListing" />
        </asp:BoundField>
        <asp:TemplateField HeaderText="Edit">
            <HeaderStyle CssClass="editHeader" />
            <ItemStyle CssClass="editListing" />
            <ItemTemplate>
                <asp:HyperLink runat="server" NavigateUrl='<%# GetEditUrl(Container.DataItem) %>' ImageUrl="~/images/edit.gif" ToolTip='<%#this.Localize("Edit.Text") %>' />
            </ItemTemplate>
        </asp:TemplateField>
        <asp:TemplateField HeaderText="Applications">
            <HeaderStyle CssClass="applicationsHeader" />
            <ItemStyle CssClass="applicationsListing" />
            <HeaderTemplate>
                        <asp:Label runat="server" resourcekey="lblApplicationsHeader" />
                    </th>
                </tr>
                <tr>
                    <th colspan="7">
                        <asp:Button ID="AddJobButton" runat="server" resourcekey="btnAddJob" />
            </HeaderTemplate>
            <ItemTemplate>
                <span><%#GetApplicationsLink(Container.DataItem)%></span>
                <asp:Repeater runat="server" DataSource='<%# GetApplicationStatusLinks((int)Eval("JobId")) %>'>
                    <HeaderTemplate><ul></HeaderTemplate>
                    <ItemTemplate>
                        <li class='<%# (bool)Eval("IsUserStatus") ? "user-status" : "application-status" %>'>
                            <asp:HyperLink runat="server" NavigateUrl='<%#Eval("Url") %>'>
                                <strong><%#Eval("Count") %></strong> <%# HttpUtility.HtmlEncode((string)Eval("Status")) %>
                            </asp:HyperLink>
                        </li>
                    </ItemTemplate>
                    <FooterTemplate></ul></FooterTemplate>
                </asp:Repeater>
            </ItemTemplate>
        </asp:TemplateField>
    </Columns>
</asp:GridView><asp:Button ID="AddJobButton" runat="server" resourcekey="btnAddJob" Visible="false" />

<h3 class="SubHead"><%=this.Localize("lblUnusedItems")%></h3>
<div class="unusedItems">
    <asp:Repeater ID="EmptyStateRepeater" runat="server">
        <HeaderTemplate>
            <table class="Normal employmentTable compositeTable">
                <tr class="headerRow">
                    <th class="stateHeader" scope="col">
                        <asp:HyperLink runat="server" resourcekey="lblEmptyStateHeader" NavigateUrl='<%#EditUrl("ManageStates") %>' />
                    </th>
                </tr>
        </HeaderTemplate>
        <ItemTemplate>
                <tr class='listingRow stateRow <%#Container.ItemIndex % 2 == 0 ? string.Empty : "alternateListingRow alternateStateRow"%>'>
                    <td class="stateListing">
                        <span><%#HttpUtility.HtmlEncode((string)Eval("StateName"))%></span>
                    </td>
                </tr>
        </ItemTemplate>
        <FooterTemplate>
            </table>
        </FooterTemplate>
    </asp:Repeater>
    <asp:Repeater ID="EmptyLocationRepeater" runat="server">
        <HeaderTemplate>
            <table class="Normal employmentTable compositeTable">
                <tr class="headerRow">
                    <th class="locationHeader" scope="col">
                        <asp:HyperLink runat="server" resourcekey="lblEmptyLocationHeader" NavigateUrl='<%#EditUrl("ManageLocations") %>' />
                    </th>
                </tr>
        </HeaderTemplate>
        <ItemTemplate>
                <tr class='listingRow locationRow <%#Container.ItemIndex % 2 == 0 ? string.Empty : "alternateListingRow alternateLocationRow"%>'>
                    <td class="locationListing">
                        <span><%#HttpUtility.HtmlEncode(string.Format(this.Localize("Location"), Eval("LocationName"), Eval("StateName"), Eval("StateAbbreviation")))%></span>
                    </td>
                </tr>
        </ItemTemplate>
        <FooterTemplate>
            </table>
        </FooterTemplate>
    </asp:Repeater>
    <asp:Repeater ID="EmptyCategoryRepeater" runat="server">
        <HeaderTemplate>
            <table class="Normal employmentTable compositeTable">
                <tr class="headerRow">
                    <th class="categoryHeader" scope="col">
                        <asp:HyperLink runat="server" resourcekey="lblEmptyCategoryHeader" NavigateUrl='<%#EditUrl("ManageCategories") %>' />
                    </th>
                </tr>
        </HeaderTemplate>
        <ItemTemplate>
                <tr class='listingRow categoryRow <%#Container.ItemIndex % 2 == 0 ? string.Empty : "alternateListingRow alternateCategoryRow"%>'>
                    <td class="categoryListing">
                        <span><%#HttpUtility.HtmlEncode((string)Eval("CategoryName"))%></span>
                    </td>
                </tr>
        </ItemTemplate>
        <FooterTemplate>
            </table>
        </FooterTemplate>
    </asp:Repeater>
    <asp:Repeater ID="EmptyPositionRepeater" runat="server">
        <HeaderTemplate>
            <table class="Normal employmentTable compositeTable">
                <tr class="headerRow">
                    <th class="positionHeader" scope="col">
                        <asp:HyperLink runat="server" resourcekey="lblEmptyPositionHeader" NavigateUrl='<%#EditUrl("ManagePositions") %>' />
                    </th>
                </tr>
        </HeaderTemplate>
        <ItemTemplate>
                <tr class='listingRow positionRow <%#Container.ItemIndex % 2 == 0 ? string.Empty : "alternateListingRow alternatePositionRow"%>'>
                    <td class="positionListing">
                        <span><%#HttpUtility.HtmlEncode((string)Eval("JobTitle"))%></span>
                    </td>
                </tr>
        </ItemTemplate>
        <FooterTemplate>
            </table>
        </FooterTemplate>
    </asp:Repeater>
</div>
<asp:LinkButton ID="BackButton" runat="server" CssClass="Normal" resourcekey="btnBack" />
